using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ghasele.Application.Interfaces
{
    /// <summary>
    /// Persists user-uploaded binary files (currently support-ticket photos) and
    /// returns a root-relative URL the clients can resolve against the API origin.
    /// The concrete implementation lives in the host so it can pick the physical
    /// location from the hosting environment.
    /// </summary>
    public interface IFileStorageService
    {
        /// <param name="folder">Sub-folder under the public uploads root, e.g. "support".</param>
        /// <returns>Root-relative URL, e.g. "/uploads/support/ab12….jpg".</returns>
        Task<string> SaveAsync(Stream content, string originalFileName, string folder, CancellationToken cancellationToken = default);
    }
}
