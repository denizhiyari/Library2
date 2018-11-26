using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Language.V1;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Grpc.Auth;
using SpeechRecognizer.GlobalHook;

namespace SpeechRecognizer
{


    public class SpeechRecognize
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);


        public SpeechRecognize()
        {

            Thread.Sleep(3000);
            SendString("ışınlanmak ğğğğ");
            //GlobalHook.HookManager.KeyDown += new System.Windows.Forms.KeyEventHandler(KeyPresshandler);


           // object obj = StreamingMicRecognizeAsync(3);


        }
        public void AuthImplicit(string projectId)
        {
            // If you don't specify credentials when constructing the client, the
            // client library will look for credentials in the environment.


        }

        async Task<object> StreamingMicRecognizeAsync(int seconds)
        {
            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                Console.WriteLine("No microphone!");
                return -1;
            }

            var credential = GoogleCredential.FromFile(@"D:\Business\SpeechRecognizer\SpeecRecognizer-b935745508aa.json")
        .CreateScoped(SpeechClient.DefaultScopes);
            var channel = new Grpc.Core.Channel(
                SpeechClient.DefaultEndpoint.ToString(),
                credential.ToChannelCredentials());

            var speech = SpeechClient.Create(channel);
            var streamingCall = speech.StreamingRecognize();
            // Write the initial request with the config.
            await streamingCall.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding =
                            RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = LanguageCodes.Turkish.Turkey
                        },
                        InterimResults = true,
                    }
                });
            // Print responses as they arrive.


            Task<string> printResponses = Task.Run(async () =>
            {
                float max = float.MinValue;
                string val = "";

                while (await streamingCall.ResponseStream.MoveNext(
                    default(CancellationToken)))
                {
                    foreach (var result in streamingCall.ResponseStream
                        .Current.Results)
                    {


                        foreach (var alternative in result.Alternatives)
                        {
                            if (alternative.Confidence > max)
                            {
                                val = alternative.Transcript;
                                max = alternative.Confidence;
                            }
                        }
                    }
                }
                
                return val;
            });
            // Read from the microphone and stream to API.
            object writeLock = new object();
            bool writeMore = true;
            var waveIn = new NAudio.Wave.WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            waveIn.DataAvailable +=
                (object sender, NAudio.Wave.WaveInEventArgs args) =>
                {
                    lock (writeLock)
                    {
                        if (!writeMore) return;
                        streamingCall.WriteAsync(
                            new StreamingRecognizeRequest()
                            {
                                AudioContent = Google.Protobuf.ByteString
                                    .CopyFrom(args.Buffer, 0, args.BytesRecorded)
                            }).Wait();
                    }
                };
            waveIn.StartRecording();
            Console.WriteLine("Speak now.");
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            // Stop recording and shut down.
            waveIn.StopRecording();
            lock (writeLock) writeMore = false;
            Console.WriteLine("bitti;");
            await streamingCall.WriteCompleteAsync();
            Console.WriteLine("bitti 2;");
            string result3 = await printResponses;
            Console.WriteLine(result3);
            return 0;
        }
        public void KeyPresshandler(object sender, KeyEventArgs e)
        {

            Keys key = e.KeyCode;
            if (key == Keys.F7)
            {

            }
            else if (key == Keys.F8)
            {

            }



        }
        public static void SendString(string s)
        {
            // Construct list of inputs in order to send them through a single SendInput call at the end.
            List<INPUT> inputs = new List<INPUT>();

            // Loop through each Unicode character in the string.
            foreach (char c in s)
            {
                // First send a key down, then a key up.
                foreach (bool keyUp in new bool[] { false, true })
                {
                    // INPUT is a multi-purpose structure which can be used 
                    // for synthesizing keystrokes, mouse motions, and button clicks.
                    INPUT input = new INPUT
                    {
                        // Need a keyboard event.
                        type = INPUT_KEYBOARD,
                        u = new InputUnion
                        {
                            // KEYBDINPUT will contain all the information for a single keyboard event
                            // (more precisely, for a single key-down or key-up).
                            ki = new KEYBDINPUT
                            {
                                // Virtual-key code must be 0 since we are sending Unicode characters.
                                wVk = 0,

                                // The Unicode character to be sent.
                                wScan = c,

                                // Indicate that we are sending a Unicode character.
                                // Also indicate key-up on the second iteration.
                                dwFlags = KEYEVENTF_UNICODE | (keyUp ? KEYEVENTF_KEYUP : 0),

                                dwExtraInfo = GetMessageExtraInfo(),
                            }
                        }
                    };

                    // Add to the list (to be sent later).
                    inputs.Add(input);
                }
            }

            // Send all inputs together using a Windows API call.
            SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(INPUT)));
        }

        const int INPUT_MOUSE = 0;
        const int INPUT_KEYBOARD = 1;
        const int INPUT_HARDWARE = 2;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_UNICODE = 0x0004;
        const uint KEYEVENTF_SCANCODE = 0x0008;
        const uint XBUTTON1 = 0x0001;
        const uint XBUTTON2 = 0x0002;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_XDOWN = 0x0080;
        const uint MOUSEEVENTF_XUP = 0x0100;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            /*Virtual Key code.  Must be from 1-254.  If the dwFlags member specifies KEYEVENTF_UNICODE, wVk must be 0.*/
            public ushort wVk;
            /*A hardware scan code for the key. If dwFlags specifies KEYEVENTF_UNICODE, wScan specifies a Unicode character which is to be sent to the foreground application.*/
            public ushort wScan;
            /*Specifies various aspects of a keystroke.  See the KEYEVENTF_ constants for more information.*/
            public uint dwFlags;
            /*The time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time stamp.*/
            public uint time;
            /*An additional value associated with the keystroke. Use the GetMessageExtraInfo function to obtain this information.*/
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        public void Initialize()
        {
        }
    }
}
