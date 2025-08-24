namespace FireInvent.Contract;

public static class Roles
{
    public const string Procurement = "procurement";
    public const string Admin = "admin";
    public const string Maintenance = "maintenance";

    public static readonly string[] AllRoles = [Procurement, Admin, Maintenance];
}
