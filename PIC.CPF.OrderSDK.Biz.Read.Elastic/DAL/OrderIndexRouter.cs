namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    /// <summary>
    /// index 名稱由 coom_no 第 4-6 碼（1-indexed）決定：
    /// coom_no = "CM2604..." → suffix = "604" → "orders-604"
    /// suffix = 年份末碼 + 月份兩碼，因此每月一個 index。
    /// </summary>
    internal static class OrderIndexRouter
    {
        private const string Prefix = "orders-";
        internal const string Wildcard = "orders-*";

        /// <summary>從 coom_no 取得所屬 index 名稱。</summary>
        internal static string FromCoomNo(string coomNo)
            => $"{Prefix}{coomNo[3..6]}";

        /// <summary>從單一日期區間算出涉及的所有月份 index。</summary>
        internal static string[] Resolve(DateTime start, DateTime end)
        {
            var indices = new List<string>();
            var current = new DateTime(start.Year, start.Month, 1);
            var last = new DateTime(end.Year, end.Month, 1);
            while (current <= last)
            {
                indices.Add($"{Prefix}{current.Year % 10}{current.Month:D2}");
                current = current.AddMonths(1);
            }
            return [.. indices];
        }

        /// <summary>從多個可為 null 的日期區間取聯集後算 index。全部為 null 則回傳萬用字元。</summary>
        internal static string[] Resolve(params (DateTime? Start, DateTime? End)[] ranges)
        {
            var starts = ranges.Select(r => r.Start).OfType<DateTime>();
            var ends   = ranges.Select(r => r.End).OfType<DateTime>();
            if (!starts.Any() || !ends.Any()) return [Wildcard];
            return Resolve(starts.Min(), ends.Max());
        }
    }
}
