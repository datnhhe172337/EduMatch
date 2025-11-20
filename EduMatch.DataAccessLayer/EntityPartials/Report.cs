using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Report
{
    [NotMapped]
    public ReportStatus StatusEnum
    {
        get => (ReportStatus)Status;
        set => Status = (int)value;
    }
}
