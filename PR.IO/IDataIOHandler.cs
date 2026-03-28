using System.Collections.Generic;
using PR.Domain.Entities;

namespace PR.IO
{
    public interface IDataIOHandler
    {
        void ExportDataToXML(
            PRData prData,
            string fileName);

        void ExportDataToJson(
            PRData prData,
            string fileName);

        void ExportDataToGraphML(
            PRData prData,
            string fileName);

        void ImportDataFromXML(
            string fileName,
            out PRData prData);

        void ImportDataFromJson(
            string fileName,
            out PRData prData);
    }
}
