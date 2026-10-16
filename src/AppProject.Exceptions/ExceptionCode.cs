namespace AppProject.Exceptions;

public enum ExceptionCode
{
    Generic,
    SecurityValidation,
    RequestValidation,
    Concurrency,
    EntityNotFound,

    // Pattern: ModuleName_EntityName_ValidationName
    General_Country_DuplicateName,
    General_State_DuplicateName,
    General_City_DuplicateName,
    General_City_Neighborhood_DuplicateName,
    Financial_FinancialAccount_DuplicateName,
    Financial_FinancialAccount_InUse,
    Financial_FinancialCategory_DuplicateName,
    Financial_FinancialCategory_InUse,
    Financial_AccountPayable_FinancialAccountRequired,
    Financial_AccountPayable_PaymentDateRequired,
    Financial_AccountPayable_FinancialCategoryNotFound,
    Financial_AccountPayable_FinancialAccountNotFound,
    Financial_AccountPayable_InvalidDueDateRange,
    Financial_AccountReceivable_FinancialAccountRequired,
    Financial_AccountReceivable_ReceiptDateRequired,
    Financial_AccountReceivable_FinancialCategoryNotFound,
    Financial_AccountReceivable_FinancialAccountNotFound,
    Financial_AccountReceivable_InvalidDueDateRange
}
