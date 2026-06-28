using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Web.Infrastructure
{
    public static class FileUpload
    {
        private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public static bool IsValidImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return false;
            var ext = Path.GetExtension(file.FileName);
            return ext != null && Allowed.Contains(ext.ToLowerInvariant());
        }

        public static System.Collections.Generic.List<string> SaveMany(System.Collections.Generic.IEnumerable<HttpPostedFileBase> files, string subfolder)
        {
            var saved = new System.Collections.Generic.List<string>();
            if (files == null)
                return saved;
            foreach (var file in files)
            {
                var path = Save(file, subfolder);
                if (path != null)
                    saved.Add(path);
            }
            return saved;
        }

        public static string Save(HttpPostedFileBase file, string subfolder)
        {
            if (!IsValidImage(file))
                return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var dir = HostingEnvironment.MapPath("~/Content/uploads/" + subfolder);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var name = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(dir, name));
            return "~/Content/uploads/" + subfolder + "/" + name;
        }
    }
}
