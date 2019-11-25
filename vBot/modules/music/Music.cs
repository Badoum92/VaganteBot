using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;
using Discord.Audio;
using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace vBot.modules.music
{
    public class VoiceChat : ModuleBase<SocketCommandContext>
    {
        static IAudioClient VCclient = null;

        static Queue<string> queue;
        static List<ulong> voted;

        static string[] currentSong;

        static Stopwatch stopwatch;

        static Process ffmpeg;
        static AudioOutStream stream;

        static bool playing;
        static int votes;

        [Command("join", RunMode = RunMode.Async)]
        public async Task Join()
        {
            if (VCclient != null)
            {
                await ReplyAsync("Bot already in use. Join the channel or wait.");
                return;
            }

            IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
            playing = false;
            VCclient = await channel.ConnectAsync();
            stream = VCclient.CreatePCMStream(AudioApplication.Music);
            queue = new Queue<string>();
            currentSong = null;
            voted = new List<ulong>();
            stopwatch = new Stopwatch();
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

            if (channel == null) return;

            if (voted.Contains(Context.User.Id))
            {
                await ReplyAsync("You have already voted");
                return;
            }

            voted.Add(Context.User.Id);

            votes++;

            await ReplyAsync(Context.User.Mention + " voted to skip the current song. " + votes + "/" + ((channel.Users.Count + 1) / 2) + " votes to skip");

            if (votes >= (channel.Users.Count + 1) / 2)
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
            string[] duration = currentSong[2].Split(':');
            int totalSec = Int32.Parse(duration[0]) * 60 + Int32.Parse(duration[1]) + 2;
            int elapsedSec = (int)Math.Floor(stopwatch.Elapsed.TotalSeconds);
            int remaining = totalSec - elapsedSec;

            int min = remaining / 60;
            int sec = remaining % 60;

            await ReplyAsync("Time remaining for **[" + currentSong[0] + "]**: ``" + min + ":" + (sec < 10 ? "0" : "") + sec + "``");
        }

        [Command("addsong", RunMode = RunMode.Async)]
        public async Task AddSong([Remainder] string url)
        {
            if (VCclient == null) return;

            int count = 0;
            foreach (string song in queue)
            {
                if (song.Split("$|£")[4] == Context.User.Username) count++;
            }

            if (count >= 4)
            {
                await ReplyAsync("You have already queued 4 songs, wait until one of your songs is played");
                return;
            }

            await SearchVideo(url);

            if (!playing) await PlaySong();
        }

        [Command("next", RunMode = RunMode.Async)]
        public async Task Next()
        {
            if (queue.Count == 0)
            {
                await ReplyAsync("This is the last song");
                return;
            }

            string str = "```";

            foreach (string song in queue)
                str += song.Split("$|£")[0] + " (" + song.Split("$|£")[2] + ")\n";

            str += "```";

            await ReplyAsync(str);
        }

        [Command("current", RunMode = RunMode.Async)]
        public async Task Current()
        {
            if (currentSong != null)
            {
                await Context.Channel.SendMessageAsync("", false, MakeSongEmbed(currentSong));
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
                playing = false;
                currentSong = null;
                return;
            }

            playing = true;
            votes = 0;
            voted = new List<ulong>();

            currentSong = queue.Dequeue().Split("$|£");

            await Context.Channel.SendMessageAsync("", false, MakeSongEmbed(currentSong));

            ffmpeg = CreateStream(currentSong[3]);

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
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = File.ReadLines("data/youtubetoken").First(),
                ApplicationName = GetType().ToString()
            });

            string endID = FormatURL(url);

            var snippetRequest = youtubeService.Videos.List("snippet");
            snippetRequest.Id = endID;
            var snippetResult = await snippetRequest.ExecuteAsync();

            var contentRequest = youtubeService.Videos.List("contentDetails");
            contentRequest.Id = endID;
            var contentResult = await contentRequest.ExecuteAsync();

            string[] duration = contentResult.Items[0].ContentDetails.Duration.Split('M');
            int min = Int32.Parse(duration[0].Split('T')[1]);
            int sec = Int32.Parse(duration[1].Split('S')[0]);

            if (min >= 30)
            {
                await ReplyAsync("The song is longer than 30 minutes, please choose another one");
            }

            if (snippetResult.Items.Count == 1)
            {
                string link = "https://www.youtube.com/watch?v=" + endID;
                string thumbnail = snippetResult.Items[0].Snippet.Thumbnails.Medium.Url;
                string title = snippetResult.Items[0].Snippet.Title;
                string length = min + ":" + sec;
                string user = Context.User.Username;
                string channel = snippetResult.Items[0].Snippet.ChannelTitle;

                queue.Enqueue(title + "$|£" + channel + "$|£" + length + "$|£" + link + "$|£" + user + "$|£" + thumbnail);
                Console.WriteLine(title + "$|£" + channel + "$|£" + length + "$|£" + link + "$|£" + user + "$|£" + thumbnail);

                await ReplyAsync("Song **[" + snippetResult.Items[0].Snippet.Title + "]** added " + Context.User.Mention);
            }
            else
            {
                await ReplyAsync("Song not found " + Context.User.Mention);
            }
        }

        public static async Task DisconnectVC()
        {
            await StopMusic();
            await VCclient.StopAsync();
            VCclient = null;
        }

        public static async Task StopMusic()
        {
            if (!playing) return;

            playing = false;
            ffmpeg.StandardOutput.BaseStream.Dispose();
            await stream.FlushAsync();
            queue = new Queue<string>();
        }

        private string FormatURL(string url)
        {
            string[] splitURL = url.Split("=");
            string endID = "";

            if (url.Contains("youtu.be"))
            {
                endID = url.Split("/")[url.Split("/").Length - 1];
            }
            else
            {
                if (splitURL.Length == 2)
                {
                    endID = splitURL[1];
                }
                else if (splitURL.Length > 2)
                {
                    endID = splitURL[1].Split("&")[0];
                }

                if (endID.Contains("&"))
                {
                    endID = endID.Split("&")[0];
                }
            }

            return endID;
        }

        private Embed MakeSongEmbed(string[] current)
        {
            EmbedBuilder embed = new EmbedBuilder();

            string name = current[0];
            string channel = current[1];
            string duration = current[2];
            string url = current[3];
            string user = current[4];
            string thumbnail = current[5];

            embed.WithColor(new Color(255, 255, 255));
            embed.AddField("Currently Playing", name);
            embed.AddField("By", channel);
            embed.AddField("Duration", duration);
            embed.AddField("URL", url);
            embed.WithImageUrl(thumbnail);
            embed.AddField("Requested by", user);

            return embed.Build();
        }
    }
}
