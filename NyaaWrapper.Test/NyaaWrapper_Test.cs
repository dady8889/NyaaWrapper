using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NyaaWrapper.Test
{
    [TestClass]
    public class NyaaWrapper_Test
    {
        private readonly Wrapper _wrapper;

        public NyaaWrapper_Test()
        {
            _wrapper = new Wrapper();
        }

        [TestMethod]
        public async Task NyaaWrapper_KimetsuNoYaiba()
        {
            var animeResults =
               await _wrapper.GetNyaaEntries(new QueryOptions { Search = "kimetsu no yaiba" });

            // take only torrent names written in this form
            // if the torrent name starts with a number, thats a bug
            var regex = new Regex("^(\\d)\\[(.*)\\](.*)$");

            foreach (var anime in animeResults)
            {
                if (regex.IsMatch(anime.Name))
                {
                    Assert.Fail($"{anime.Name}");
                }
            }
        }
    }
}
