namespace FireInvent.Contract;

public static class Roles
{
    public const string Procurement = "procurement";
    public const string Admin = "admin";
    public const string Maintenance = "maintenance";
    public const string Integration = "integration";

    public static readonly string[] AllRoles = [Procurement, Admin, Maintenance, Integration];
}
