using MongoDB.Bson;
using NO3._dbSDK_Imporve.Application.DTO;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using System.Text.Json;

namespace NO3._dbSDK_Imporve.Application.Services
{
    /// <summary>
    /// 貨態同步服務：協調 MongoDB 與 Elasticsearch 的雙端同步
    /// </summary>
    public class ShippingSyncService
    {
        private readonly OrderRepository_Mongo _mongoRepo;
        private readonly IRepository<OrderSummary>? _elasticRepo;

        public ShippingSyncService(
            OrderRepository_Mongo mongoRepo,
            IRepository<OrderSummary>? elasticRepo = null)
        {
            _mongoRepo = mongoRepo;
            _elasticRepo = elasticRepo;
        }

        /// <summary>
        /// 處理貨態更新 (Status 20 或 Status 30)
        /// </summary>
        /// <param name="dto">貨態更新 DTO</param>
        /// <returns>執行結果</returns>
        public async Task<IResult> ProcessShippingUpdateAsync(ShippingUpdateDto dto)
        {
            try
            {
                // 1. 解析管線字串
                var esmlList = dto.ParseEsmlList();
                var esmsList = dto.ParseEsmsList();

                if (esmlList.Count == 0 && esmsList.Count == 0)
                {
                    return Result.SetErrorResult("ProcessShippingUpdateAsync", "無有效的貨態歷程資料");
                }

                // 2. 判斷是初始化 (Status 20) 還是追加 (Status 30)
                var coomStatus = dto.CoomStatus?.ToString() ?? "";
                var isNewShipment = coomStatus == "20";

                // 3. 建立查詢條件
                var condition = $"{{\"coom_no\": \"{dto.CoomNo}\"}}";

                // 4. 建立更新資料
                var updateData = BuildUpdateData(dto, esmlList, esmsList);

                // 5. 建立更新選項
                var options = BuildUpdateOptions(dto, esmlList, esmsList, isNewShipment);

                // 6. 執行 MongoDB 更新
                var mongoResult = await _mongoRepo.UpdateData(condition, updateData, options);

                if (!mongoResult.IsSuccess)
                {
                    return mongoResult;
                }

                // 7. 同步更新 Elasticsearch (如果有設定)
                if (_elasticRepo != null)
                {
                    var elasticResult = await SyncToElasticAsync(dto, esmlList, esmsList);
                    if (!elasticResult.IsSuccess)
                    {
                        // 記錄但不中斷流程
                        Console.WriteLine($"[Warning] Elasticsearch 同步失敗: {elasticResult.Msg}");
                    }
                }

                return Result.SetResult($"[ShippingSyncService] 貨態更新成功 (Status: {coomStatus})", mongoResult.DataJson);
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult("ProcessShippingUpdateAsync", ex.Message);
            }
        }

        /// <summary>
        /// 建立更新資料物件
        /// </summary>
        private Orders BuildUpdateData(ShippingUpdateDto dto, List<E_Shipment_L_Dto> esmlList, List<E_Shipment_S_Dto> esmsList)
        {
            return new Orders
            {
                PK = dto.CoomNo,
                C_Order_M = new C_Order_M_Model
                {
                    CoomStatus = dto.CoomStatus?.ToString()
                },
                E_Shipment_M = new E_Shipment_M_Model
                {
                    EsmmNo = dto.EsmmNo,
                    EsmmShipNo = dto.EsmmShipNo,
                    EsmmStatus = dto.EsmmStatus?.ToString(),
                    EsmmShipMethod = dto.EsmmShipMethod?.ToString(),
                    EsmmShipNoAuthCode = dto.EsmmShipNoAuthCode?.ToString(),
                    EsmmShipNoA = dto.EsmmShipNoA,
                    EsmmIbonAppFlag = dto.EsmmIbonAppFlag?.ToString()
                },
                // 陣列資料會透過 $push 處理，這裡只放要 $set 的部分
                E_Shipment_L = new List<E_Shipment_L_Model>(),
                E_Shipment_S = new List<E_Shipment_S_Model>()
            };
        }

        /// <summary>
        /// 建立更新選項 (包含 $push 設定)
        /// </summary>
        private MongoUpdateOptions BuildUpdateOptions(
            ShippingUpdateDto dto,
            List<E_Shipment_L_Dto> esmlList,
            List<E_Shipment_S_Dto> esmsList,
            bool isNewShipment)
        {
            var options = new MongoUpdateOptions
            {
                IsUpsert = true,
                ReturnNewDocument = true
            };

            // 如果是追加模式 (Status 30)，設定 $push
            if (!isNewShipment && esmlList.Count > 0)
            {
                options.PushFields = new Dictionary<string, BsonValue>();

                // 追加貨態歷程
                if (esmlList.Count > 0)
                {
                    var esmlArray = new BsonArray();
                    foreach (var item in esmlList)
                    {
                        esmlArray.Add(new BsonDocument
                        {
                            { "esml_esmm_status", item.EsmlEsmmStatus ?? "" },
                            { "esml_status_datetime", item.EsmlStatusDatetime ?? DateTime.UtcNow }
                        });
                    }
                    options.PushFields["e_shipment_l"] = esmlArray;
                }

                // 追加物流狀態
                if (esmsList.Count > 0)
                {
                    var esmsArray = new BsonArray();
                    foreach (var item in esmsList)
                    {
                        esmsArray.Add(new BsonDocument
                        {
                            { "esms_dlv_status_no", item.EsmsDlvStatusNo ?? "" },
                            { "esms_status_datetime", item.EsmsStatusDatetime ?? DateTime.UtcNow }
                        });
                    }
                    options.PushFields["e_shipment_s"] = esmsArray;
                }
            }

            return options;
        }

        /// <summary>
        /// 同步更新 Elasticsearch
        /// </summary>
        private async Task<IResult> SyncToElasticAsync(
            ShippingUpdateDto dto,
            List<E_Shipment_L_Dto> esmlList,
            List<E_Shipment_S_Dto> esmsList)
        {
            if (_elasticRepo == null)
                return Result.SetResult("Elasticsearch 未設定");

            try
            {
                var condition = $"{{\"coom_no\": \"{dto.CoomNo}\"}}";

                var elasticData = new OrderSummary
                {
                    PK = dto.CoomNo,
                    CoomStatus = dto.CoomStatus?.ToString(),
                    EsmmShipNo = dto.EsmmShipNo,
                    EsmmStatus = dto.EsmmStatus?.ToString()
                };

                // 如果有最新的貨態歷程，更新寄貨時間
                if (esmlList.Count > 0)
                {
                    var latestEsml = esmlList.OrderByDescending(x => x.EsmlStatusDatetime).First();
                    if (latestEsml.EsmlEsmmStatus == "10")
                    {
                        elasticData.EsmlStatusShippingDatetime = latestEsml.EsmlStatusDatetime;
                    }
                }

                return await _elasticRepo.UpdateData(condition, elasticData);
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult("SyncToElasticAsync", ex.Message);
            }
        }

        /// <summary>
        /// 初始化更新指令 (供 Mock 測試使用)
        /// </summary>
        public async Task<IResult> InitUpdateAsync(ShippingUpdateDto dto)
        {
            var esmlList = dto.ParseEsmlList();
            var esmsList = dto.ParseEsmsList();
            var coomStatus = dto.CoomStatus?.ToString() ?? "";
            var isNewShipment = coomStatus == "20";

            var condition = $"{{\"coom_no\": \"{dto.CoomNo}\"}}";
            var updateData = BuildUpdateData(dto, esmlList, esmsList);
            var options = BuildUpdateOptions(dto, esmlList, esmsList, isNewShipment);

            // 使用 UpdateInit 取得指令 (不實際執行)
            if (_mongoRepo is OrderRepository_Mongo orderRepo)
            {
                return await orderRepo.UpdateInit(condition, updateData, options);
            }

            return Result.SetErrorResult("InitUpdateAsync", "Repository 不支援 UpdateInit");
        }
    }
}
