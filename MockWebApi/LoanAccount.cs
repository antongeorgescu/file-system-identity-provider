using System;

namespace MockCbsService
{
    public class LoanAccount
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }

        public double LoanBalance { get; set; }

        public int LoanMonths { get; set; }

    }
}
