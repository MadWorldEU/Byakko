namespace MadWorldEU.Byakko.Correspondences;

public interface ICorrespondenceService
{
    Task<Result> SendToAdministratorAsync(string title, string message);
}