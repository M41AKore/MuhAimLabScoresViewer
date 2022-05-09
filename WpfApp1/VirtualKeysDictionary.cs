using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    internal class VirtualKeysDictionary
    {
        public static Dictionary<string, int> VirtualKeys = new Dictionary<string, int>()
        {
            { "A", 0x41 },
            { "B", 0x42 },
            { "C", 0x43 },
            { "D", 0x44 },
            { "E", 0x45 },
            { "F", 0x46 },
            { "G", 0x47 },
            { "H", 0x48 },
            { "I", 0x49 },
            { "J", 0x4A },
            { "K", 0x4B },
            { "L", 0x4C },
            { "M", 0x4D },
            { "N", 0x4E },
            { "O", 0x4F },
            { "P", 0x50 },
            { "Q", 0x51 },
            { "R", 0x52 },
            { "S", 0x53 },
            { "T", 0x54 },
            { "U", 0x55 },
            { "V", 0x56 },
            { "W", 0x57 },
            { "X", 0x58 },
            { "Y", 0x59 },
            { "Z", 0x5A },

            { "VK_LBUTTON", 0x01 }, //m1
            { "VK_RBUTTON", 0x02 }, //m2
            { "VK_CANCEL", 0x03 },
            { "VK_MBUTTON", 0x04 }, //m3

            { "VK_XBUTTON1", 0x05 },
            { "VK_XBUTTON2", 0x06 },

            { "VK_BACK", 0x08 }, //backspace
            { "VK_TAB", 0x09 }, //tab
            { "VK_CLEAR", 0x0C },
            { "VK_RETURN", 0x0D }, //enter
            { "VK_SHIFT", 0x10 }, //shift
            { "VK_CONTROL", 0x11 }, //ctrl
            { "VK_MENU", 0x12 }, //alt

            { "VK_PAUSE", 0x13 },
            { "VK_CAPITAL", 0x14 }, //caps lock

            { "VK_ESCAPE", 0x1B }, //esc

            { "VK_SPACE", 0x20 }, 
            { "VK_PRIOR", 0x21 }, 
            { "VK_NEXT", 0x22 }, 
            { "VK_END", 0x23 }, 
            { "VK_HOME", 0x24 }, 
            { "VK_LEFT", 0x25 }, //left arrow
            { "VK_UP", 0x26 }, 
            { "VK_RIGHT", 0x27 }, 
            { "VK_DOWN", 0x28 }, 

            { "VK_SNAPSHOT", 0x2C }, 
            { "VK_INSERT", 0x2D }, 
            { "VK_DELETE", 0x2E }, 
            { "VK_HELP", 0x2F }, 

            { "0", 0x30 }, 
            { "1", 0x31 }, 
            { "2", 0x32 }, 
            { "3", 0x33 }, 
            { "4", 0x34 }, 
            { "5", 0x35 },
            { "6", 0x36 }, 
            { "7", 0x37 }, 
            { "8", 0x38 }, 
            { "9", 0x39 }, 

            { "NUMPAD0", 0x60 }, 
            { "NUMPAD1", 0x61 }, 
            { "NUMPAD2", 0x62 }, 
            { "NUMPAD3", 0x63 }, 
            { "NUMPAD4", 0x64 }, 
            { "NUMPAD5", 0x65 }, 
            { "NUMPAD6", 0x66 }, 
            { "NUMPAD7", 0x67 }, 
            { "NUMPAD8", 0x68 }, 
            { "NUMPAD9", 0x69 }, 

            { "MULTIPLY", 0x6A }, 
            { "ADD", 0x6B }, 
            { "SEPARATOR", 0x6C }, 
            { "SUBTRACT", 0x6D }, 
            { "DECIMAL", 0x6E }, 
            { "DIVIDE", 0x6F },
            
            { "F1", 0x70 }, 
            { "F2", 0x71 }, 
            { "F3", 0x72 }, 
            { "F4", 0x73 }, 
            { "F5", 0x74 }, 
            { "F6", 0x75 }, 
            { "F7", 0x76 }, 
            { "F8", 0x77 }, 
            { "F9", 0x78 }, 
            { "F10", 0x79 }, 
            { "F11", 0x7A }, 
            { "F12", 0x7B }, 
            { "F13", 0x7C }, 
            { "F14", 0x7D }, 
            { "F15", 0x7E }, 
            { "F16", 0x7F }, 
            { "F17", 0x80 }, 
            { "F18", 0x81 }, 
            { "F19", 0x82 }, 
            { "F20", 0x83 }, 
            { "F21", 0x84 }, 
            { "F22", 0x85 }, 
            { "F23", 0x86 }, 
            { "F24", 0x87 }, 

            { "VK_NUMLOCK", 0x90 }, 
            { "VK_SCROLL", 0x91 }, 

            { "VK_LSHIFT", 0xA0 }, 
            { "VK_RSHIFT", 0xA1 }, 
            { "VK_LCONTROL", 0xA2 }, 
            { "VK_RCONTROL", 0xA3 }, 
            { "VK_LMENU", 0xA4 }, //alt
            { "VK_RMENU", 0xA5 }, 

            { "VK_OEM_1", 0xBA }, //Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the ';:' key		
            { "VK_OEM_PLUS", 0xBB }, //For any country/region, the '+' key

        };

	/*	
VK_OEM_COMMA	0xBC	For any country/region, the ',' key
VK_OEM_MINUS	0xBD	For any country/region, the '-' key
VK_OEM_PERIOD	0xBE	For any country/region, the '.' key
VK_OEM_2	0xBF	Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the '/?' key
VK_OEM_3	0xC0	Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the '`~' key
-	0xC1-D7 Reserved
-	0xD8-DA Unassigned
VK_OEM_4	0xDB	Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the '[{' key
VK_OEM_5	0xDC	Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the '\|' key
VK_OEM_6	0xDD	Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the ']}' key
VK_OEM_7	0xDE	Used for miscellaneous characters; it can vary by keyboard.For the US standard keyboard, the 'single-quote/double-quote' key
VK_OEM_8	0xDF	Used for miscellaneous characters; it can vary by keyboard.
-	0xE0	Reserved
0xE1	OEM specific
VK_OEM_102	0xE2	The<> keys on the US standard keyboard, or the \\| key on the non-US 102-key keyboard
0xE3-E4 OEM specific
VK_PROCESSKEY	0xE5	IME PROCESS key
0xE6	OEM specific
VK_PACKET	0xE7	Used to pass Unicode characters as if they were keystrokes.The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
-	0xE8	Unassigned
0xE9-F5 OEM specific
VK_ATTN	0xF6	Attn key
VK_CRSEL	0xF7	CrSel key
VK_EXSEL	0xF8	ExSel key
VK_EREOF	0xF9	Erase EOF key
VK_PLAY	0xFA	Play key
VK_ZOOM	0xFB	Zoom key
VK_NONAME	0xFC	Reserved
VK_PA1	0xFD	PA1 key
VK_OEM_CLEAR	0xFE	Clear key*/
        public static int getVirtualKey(string keystring)
        {

            VirtualKeys.TryGetValue(keystring.ToUpper(), out int result);

            return result;
        }
    }
}
