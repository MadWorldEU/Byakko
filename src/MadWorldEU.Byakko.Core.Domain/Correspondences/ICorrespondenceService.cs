namespace MadWorldEU.Byakko.Correspondences;

public interface ICorrespondenceService
{
    Result SendToAdministrator(string title, string message);
}