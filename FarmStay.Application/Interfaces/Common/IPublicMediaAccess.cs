namespace FarmStay.Application.Interfaces.Common;
public interface IPublicMediaAccess { Task<bool> CanReadAsync(string path,CancellationToken ct=default); }
