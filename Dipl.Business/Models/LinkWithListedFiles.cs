using Dipl.Business.Entities;
using Dipl.Business.Services.Interfaces;
using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Models;


/// <summary>
/// `Link` class currently does not list the files that are in storage, so we need to extend it and
/// add the `Files` property which is loaded from the storage.
/// </summary>
public class LinkWithListedFiles : Link
{
    public required FileInfo[] Files { get; set; }

    public static LinkWithListedFiles FromLink(Link link, FileInfo[] fileInfos)
    {
        return new LinkWithListedFiles
        {
            LinkId = link.LinkId,
            Folder = link.Folder,
            LinkType = link.LinkType,
            CreatedAt = link.CreatedAt,
            CreatedById = link.CreatedById,
            CreatedBy = link.CreatedBy,
            Groups = link.Groups,
            Files = fileInfos,
        };
    }
}