using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;

namespace BloggerToMd
{
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option("-src")]
        [Required]
        public string Source { get; }

        public void OnExecute()
        {
            BloggerToMarkdown.Convert(Source);
        }
    }
}
