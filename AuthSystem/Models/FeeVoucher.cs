using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class FeeVoucher
    {
        [Key]
        public int Id { get; set; }

        public int VoucherNumber { get; set; }
        public int Amount { get; set; }
        public int TestId { get; set; }
        public string ApplicantName { get; set; }
        public string TestName { get; set; }
        public bool isPaid { get; set; }
    }
}