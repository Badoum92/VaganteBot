using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Discord.WebSocket;
using Discord;
using Discord.Audio;
using System.Diagnostics;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace vBot.modules.music
{
    public class VoiceChat : ModuleBase<SocketCommandContext>
    {
        static IVoiceChannel voiceChannel = null;
        static IAudioClient VCclient = null;
        static Queue<Song> queue;
        static List<ulong> votes;
        static Song currentSong = null;
        static Stopwatch stopwatch;
        static Process ffmpeg;
        static AudioOutStream stream;
        static YouTubeService youtubeService;

        class Song
        {
            public string url;
            public string thumbnailUrl;
            public string title;
            public string ytChannel;
            public int durationSec;
            public SocketGuildUser requester;

            public Song(string url, string thumbnailUrl, string title, string ytChannel, int durationSec, SocketGuildUser requester)
            {
                this.url = url;
                this.thumbnailUrl = thumbnailUrl;
                this.title = title;
                this.ytChannel = ytChannel;
                this.durationSec = durationSec;
                this.requester = requester;
            }

            public override string ToString()
            {
                string s = "Added song =\n{\n";
                s += "    URL: " + url + "\n";
                s += "    Title: " + title + "\n";
                s += "    Channel: " + ytChannel + "\n";
                s += "    Duration: " + FormatTime(durationSec) + "\n";
                s += "    Requested by: " + requester.Username + "\n";
                s += "}";
                return s;
            }

            public Embed ToEmbed()
            {
                EmbedBuilder embed = new EmbedBuilder();

                embed.WithColor(new Color(255, 255, 255));
                embed.AddField("Currently Playing", title);
                embed.AddField("By", ytChannel);
                embed.AddField("Duration", FormatTime(durationSec));
                embed.AddField("URL", url);
                embed.WithImageUrl(thumbnailUrl);
                embed.AddField("Requested by", requester.Username);

                return embed.Build();
            }
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task ForceJoin()
        {
            if (Context.User.Id != Program.owner) return;
            await Join();
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task Leave()
        {
            if (Context.User.Id != Program.owner) return;
            await DisconnectVC();
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task Play()
        {
            if (Context.User.Id != Program.owner) return;
            await PlaySong();
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task Stop()
        {
            if (Context.User.Id != Program.owner) return;
            await StopMusic();
        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            SocketVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel as SocketVoiceChannel;

            if (channel == null)
                return;

            if (votes.Contains(Context.User.Id))
            {
                await ReplyAsync("You have already voted");
                return;
            }

            votes.Add(Context.User.Id);

            await ReplyAsync(Context.User.Mention + " voted to skip the current song. " + votes.Count + "/" + ((channel.Users.Count + 1) / 2) + " votes to skip");

            if (votes.Count >= (channel.Users.Count + 1) / 2)
            {
                await ReplyAsync("Skipped!");
                ffmpeg.StandardOutput.BaseStream.Dispose();
                await stream.FlushAsync();
                await PlaySong();
            }
        }

        [Command("forceskip", RunMode = RunMode.Async)]
        public async Task ForceSkip()
        {
            if (Context.User.Id != Program.owner) return;
            await ReplyAsync("Force Skipped!");
            ffmpeg.StandardOutput.BaseStream.Dispose();
            await stream.FlushAsync();
            await PlaySong();
        }

        [Command("remaining", RunMode = RunMode.Async)]
        public async Task Remaning()
        {
            if (currentSong == null)
            {
                await ReplyAsync("There is no song currently playing.");
                return;
            }

            int elapsedSec = (int)Math.Floor(stopwatch.Elapsed.TotalSeconds);
            int totalSec = currentSong.durationSec;
            int remainingSec = totalSec - elapsedSec;
            string elapsedStr = FormatTime(elapsedSec);
            string totalStr = FormatTime(totalSec);
            string remainingStr = FormatTime(remainingSec);

            float percent = (elapsedSec * 100.0f / totalSec) / 100.0f;
            int totalSymbols = 40;
            float nbSymbols = totalSymbols * percent;

            string s = Util.FormatText(currentSong.title, "**") + ": " + remainingStr + " remaining\n";
            s += "`00:00 - [";
            for (int i = 0; i < nbSymbols; i++)
            {
                s += "=";
            }
            s += "(" + elapsedStr + ")";
            for (int i = 0; i < totalSymbols - nbSymbols; i++)
            {
                s += " ";
            }
            s += "] - " + totalStr + "`";

            await ReplyAsync(s);
        }

        [Command("addsong", RunMode = RunMode.Async)]
        public async Task AddSong([Remainder] string url)
        {
            if (VCclient == null)
            {
                await Join();
                if (VCclient == null)
                    return;
            }

            IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
            if (channel == null || channel.Id != voiceChannel.Id)
            {
                await ReplyAsync(Util.FormatText(Context.User.Username, "**") + ", you need to be in the same channel as the bot.");
                return;
            }

            int count = 0;
            foreach (var song in queue)
            {
                if (song.requester.Id == Context.User.Id)
                    count++;
            }

            int maxUserSongs = 10;
            if (count >= maxUserSongs)
            {
                await ReplyAsync(Util.FormatText(Context.User.Username, "**") + ", you have already queued " + maxUserSongs + " songs, wait until one of your songs is played");
                return;
            }

            await SearchVideo(url);

            if (currentSong == null)
                await PlaySong();
        }

        [Command("next", RunMode = RunMode.Async)]
        public async Task Next()
        {
            if (currentSong == null)
            {
                await ReplyAsync("There is not song playing");
                return;
            }

            if (queue.Count == 0)
            {
                await ReplyAsync("This is the last song");
                return;
            }

            string str = "Next songs:\n";
            int i = 0;
            foreach (var song in queue)
            {
                str += "▫️ | " + Util.FormatText(song.title, "**") + " - " + FormatTime(song.durationSec) + "\n";
                i++;
                if (i == 10)
                    break;
            }

            await ReplyAsync(str);
        }

        [Command("current", RunMode = RunMode.Async)]
        public async Task Current()
        {
            if (currentSong != null)
            {
                await Context.Channel.SendMessageAsync("", false, currentSong.ToEmbed());
            }
            else
            {
                await ReplyAsync("No song currently playing");
            }
        }

        private async Task PlaySong()
        {
            stopwatch.Stop();
            stopwatch.Reset();

            if (queue.Count == 0)
            {
                currentSong = null;
                await DisconnectVC();
                return;
            }

			votes.Clear();
            currentSong = queue.Dequeue();
            await Context.Channel.SendMessageAsync("", false, currentSong.ToEmbed());
            ffmpeg = CreateStream(currentSong.url);
            stopwatch.Start();

            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
            await stream.FlushAsync();
            await PlaySong();
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --quiet -o - {path} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        private async Task SearchVideo(string url)
        {
            string videoID = GetVideoID(url);
            if (videoID == "")
            {
                await ReplyAsync(Util.FormatText(Context.User.Username, "**") + ", could not find requested song.");
                return;
            }
			
            var snippetRequest = youtubeService.Videos.List("snippet,contentDetails");
            snippetRequest.Id = videoID;
            var snippetResult = await snippetRequest.ExecuteAsync();

            if (snippetResult.Items.Count == 0)
            {
                await ReplyAsync(Util.FormatText(Context.User.Username, "**") + ", could not find requested song.");
                return;
            }

            int durationSec = GetDurationSec(snippetResult.Items[0].ContentDetails.Duration);
			
            if (durationSec >= 1200)
            {
                await ReplyAsync(Util.FormatText(Context.User.Username, "**") + "The song is longer than 20 minutes, please choose another one");
            }

            if (snippetResult.Items.Count == 1)
            {
                string videoUrl = "https://www.youtube.com/watch?v=" + videoID;
                string thumbnailUrl = snippetResult.Items[0].Snippet.Thumbnails.Medium.Url;
                string title = snippetResult.Items[0].Snippet.Title;
                SocketGuildUser requester = Context.User as SocketGuildUser;
                string ytChannel = snippetResult.Items[0].Snippet.ChannelTitle;

                Song song = new Song(videoUrl, thumbnailUrl, title, ytChannel, durationSec, requester);
                queue.Enqueue(song);
                Console.WriteLine(song);

                await ReplyAsync("Song **[" + title + "]** added " + Context.User.Mention);
            }
            else
            {
                await ReplyAsync("More than one result found " + Context.User.Username + ", not adding anything");
            }
        }

        public async Task Join()
        {
            if (VCclient != null)
            {
                await ReplyAsync("Bot already in use. Join the channel or wait.");
                return;
            }

            voiceChannel = (Context.User as IVoiceState).VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync(Util.FormatText(Context.User.Username, "**") + ", you are not in a voice channel.");
                return;
            }

            VCclient = await voiceChannel.ConnectAsync();
            stream = VCclient.CreatePCMStream(AudioApplication.Music);
            currentSong = null;
            queue = new Queue<Song>();
            votes = new List<ulong>();
            stopwatch = new Stopwatch();
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = File.ReadLines("data/youtubetoken").First(),
                ApplicationName = GetType().ToString()
            });
        }

        public static async Task DisconnectVC()
        {
            await StopMusic();
            await VCclient.StopAsync();
            VCclient = null;
        }

        public static async Task StopMusic()
        {
            if (currentSong == null)
                return;

            currentSong = null;
            ffmpeg.StandardOutput.BaseStream.Dispose();
            await stream.FlushAsync();
            queue.Clear();
        }

        private static string GetVideoID(string url)
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            string ret = "";
            if (query != null && query.AllKeys.Contains("v"))
            {
                ret = query["v"];
            }
            return ret;
        }

        private static int GetDurationSec(string s)
        {
            int min = 0;
            int sec = 0;
            int hour = 0;

            s = s.Split('T')[1];
            if (s.Contains('H'))
            {
                string[] split = s.Split('H');
                hour = Int32.Parse(split[0]);
                s = split[1];
            }
            if (s.Contains('M'))
            {
                string[] split = s.Split('M');
                min = Int32.Parse(split[0]);
                s = split[1];
            }
            if (s.Contains('S'))
            {
                string[] split = s.Split('S');
                sec = Int32.Parse(split[0]);
                s = split[1];
            }

            return sec + min * 60 + hour * 3600;
        }

        public static string FormatTime(int totalSec)
        {
            int min = totalSec / 60;
            int sec = totalSec - (min * 60);
            string s = "";
            if (min < 10)
                s += "0";
            s += min + ":";
            if (sec < 10)
                s += "0";
            s += sec;
            return s;
        }
    }
}
