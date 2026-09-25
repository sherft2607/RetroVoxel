using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetroVoxel
{
    public sealed class RetroVoxelClient
    {
        #region Constructor and setup

        // ONE HttpClient for the whole plugin — shared across every component and every solve.
        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://retroachievements.org/API/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Every entry in call-library-validated.json uses auth_in "query:y" — RA's web API key
        // always travels as a ?y=<key> query param, never a header.
        private const string AuthIn = "query:y";

        private readonly string _token;

        public RetroVoxelClient(string token)
        {
            _token = (token ?? "").Trim();
        }

        // relativeUrl NEVER starts with '/': with BaseAddress ".../API/", a leading slash makes
        // .NET drop the "/API" path segment and request the origin root (silent 404s).
        private HttpRequestMessage NewRequest(HttpMethod method, string relativeUrl)
        {
            relativeUrl = (relativeUrl ?? "").TrimStart('/');
            var req = new HttpRequestMessage(method, ApplyQueryAuth(relativeUrl));
            ApplyAuth(req);
            return req;
        }

        private void ApplyAuth(HttpRequestMessage req)
        {
            // AuthIn is always "query:y" for this plugin — nothing to add as a header.
        }

        private string ApplyQueryAuth(string relativeUrl)
        {
            string param = AuthIn.Substring("query:".Length);
            string sep = relativeUrl.Contains("?") ? "&" : "?";
            return relativeUrl + sep + Uri.EscapeDataString(param) + "=" + Uri.EscapeDataString(_token);
        }

        // Every path segment and query value goes through this — never string-concatenate raw input
        private static string Esc(string value) => Uri.EscapeDataString(value ?? "");

        #endregion

        #region Console methods

        // GetConsoleIDs — retro console catalog (Ingest / Presets: RA Console)
        public async Task<Tuple<bool, string, string>> GetConsoleIdsAsync(bool? activeOnly = null, bool? gamingOnly = null)
        {
            try
            {
                var url = "API_GetConsoleIDs.php";
                if (activeOnly.HasValue) url += (url.Contains("?") ? "&" : "?") + "a=" + (activeOnly.Value ? "1" : "0");
                if (gamingOnly.HasValue) url += (url.Contains("?") ? "&" : "?") + "g=" + (gamingOnly.Value ? "1" : "0");

                var req = NewRequest(HttpMethod.Get, url);
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Console methods (continued)

        // GetGameList — every game for a console, with title/ID (Find Game name lookup)
        public async Task<Tuple<bool, string, string>> GetGameListAsync(string consoleId, bool? onlyWithAchievements = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(consoleId)) return Fail("Console ID is empty.");

                var url = "API_GetGameList.php?i=" + Esc(consoleId);
                if (onlyWithAchievements.HasValue) url += "&f=" + (onlyWithAchievements.Value ? "1" : "0");

                var req = NewRequest(HttpMethod.Get, url);
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Game methods

        // GetGame — basic game metadata + image paths (Laser Standoff Stack aspect ratio source)
        public async Task<Tuple<bool, string, string>> GetGameAsync(string gameId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gameId)) return Fail("Game ID is empty.");

                var req = NewRequest(HttpMethod.Get, "API_GetGame.php?i=" + Esc(gameId));
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        // GetGameExtended — full game metadata + Achievements[] with BadgeName (RA Game Info)
        public async Task<Tuple<bool, string, string>> GetGameExtendedAsync(string gameId, int? flags = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gameId)) return Fail("Game ID is empty.");

                var url = "API_GetGameExtended.php?i=" + Esc(gameId);
                if (flags.HasValue) url += "&f=" + Esc(flags.Value.ToString());

                var req = NewRequest(HttpMethod.Get, url);
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Image downloads

        // Badge/box-art images live on the RA media CDN, not the API host, and need no auth —
        // downloaded through the shared HttpClient so components never new one up directly.
        public async Task<Tuple<bool, string, string>> DownloadImageToFileAsync(string absoluteUrl, string destPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(absoluteUrl)) return Fail("Image URL is empty.");
                using (var res = await _http.GetAsync(new Uri(absoluteUrl)).ConfigureAwait(false))
                {
                    if (!res.IsSuccessStatusCode) return Fail("HTTP " + (int)res.StatusCode + " downloading " + absoluteUrl);
                    var bytes = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    System.IO.File.WriteAllBytes(destPath, bytes);
                    return Ok(destPath);
                }
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Private helpers

        // Single send path: 429 backoff (honours Retry-After, max 3 retries), 401 surfaced as
        // "token expired / invalid", every other non-2xx returned with the body for diagnosis
        private async Task<Tuple<bool, string, string>> SendAsync(HttpRequestMessage template)
        {
            for (int attempt = 0; ; attempt++)
            {
                int status; string reason; string body; TimeSpan? retryAfter;

                using (var req = await CloneAsync(template).ConfigureAwait(false))
                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    status     = (int)res.StatusCode;
                    reason     = res.ReasonPhrase;
                    body       = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    retryAfter = res.Headers.RetryAfter?.Delta;
                }

                if (status == 429 && attempt < 3)
                {
                    var delay = retryAfter ?? TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    if (delay > TimeSpan.FromSeconds(30)) delay = TimeSpan.FromSeconds(30);
                    await Task.Delay(delay).ConfigureAwait(false);
                    continue;
                }

                if (status == 401)
                    return Fail("HTTP 401 — token expired or invalid. Get a fresh RetroAchievements web API key and re-run.\n" + body);

                if (status < 200 || status > 299)
                    return Fail("HTTP " + status + " " + reason + "\n" + body);

                return Ok(body);
            }
        }

        // HttpRequestMessage can only be sent once — always send a clone of the template
        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            foreach (var h in req.Headers) clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
            if (req.Content != null)
            {
                string body = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
                clone.Content = new StringContent(body, Encoding.UTF8,
                    req.Content.Headers.ContentType?.MediaType ?? "application/json");
            }
            return clone;
        }

        private static Tuple<bool, string, string> Ok(string body)
            => new Tuple<bool, string, string>(true, body, "");

        private static Tuple<bool, string, string> Fail(string error)
            => new Tuple<bool, string, string>(false, "", error);

        #endregion
    }
}
