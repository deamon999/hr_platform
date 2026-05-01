using System.ComponentModel;

namespace HrPlatform.Data.Enums;

public enum EmploymentType
{
    [Description("W2")] EmploymentW2,
    [Description("1099")] Employment1099
}