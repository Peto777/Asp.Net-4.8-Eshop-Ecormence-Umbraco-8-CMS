namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class CKEditorModel
    {
        public string CKEditorClass { get; set; }
        public string CKEditorHeight { get; set; }

        public CKEditorModel(string css, string height)
        {
            this.CKEditorClass = css;
            this.CKEditorHeight = height;
        }
    }
}
