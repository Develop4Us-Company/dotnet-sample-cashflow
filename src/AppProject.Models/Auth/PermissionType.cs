namespace AppProject.Models.Auth;

public enum PermissionType
{
    // Format: Module_Action
    System_ManageSettings = 1,
    Financial_ManageFinancialAccounts = 2,
    Financial_ManageFinancialCategories = 3,
    Financial_ManageAccountsPayable = 4,
    Financial_ManageAccountsReceivable = 5,
    Financial_ViewFinancialAccountBalances = 6,
}
