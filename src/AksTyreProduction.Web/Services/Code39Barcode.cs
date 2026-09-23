using System.Text;

namespace AksTyreProduction.Web.Services;
public static class Code39Barcode
{
    private static readonly Dictionary<char,int> Codes=new(){['0']=0x034,['1']=0x121,['2']=0x061,['3']=0x160,['4']=0x031,['5']=0x130,['6']=0x070,['7']=0x025,['8']=0x124,['9']=0x064,['A']=0x109,['B']=0x049,['C']=0x148,['D']=0x019,['E']=0x118,['F']=0x058,['G']=0x00D,['H']=0x10C,['I']=0x04C,['J']=0x01C,['K']=0x103,['L']=0x043,['M']=0x142,['N']=0x013,['O']=0x112,['P']=0x052,['Q']=0x007,['R']=0x106,['S']=0x046,['T']=0x016,['U']=0x181,['V']=0x0C1,['W']=0x1C0,['X']=0x091,['Y']=0x190,['Z']=0x0D0,['-']=0x085,['.']=0x184,[' ']=0x0C4,['*']=0x094};
    public static string RenderSvg(string value)
    {
        value="*"+value.ToUpperInvariant()+"*";var bars=new List<(int x,int width)>();var x=12;
        foreach(var ch in value){if(!Codes.TryGetValue(ch,out var code))continue;for(var i=0;i<9;i++){var width=(code&(1<<(8-i)))!=0?5:2;if(i%2==0)bars.Add((x,width));x+=width;}x+=2;}
        var s=new StringBuilder($"<svg xmlns='http://www.w3.org/2000/svg' width='{x+12}' height='90' viewBox='0 0 {x+12} 90'><rect width='100%' height='100%' fill='white'/>");foreach(var bar in bars)s.Append($"<rect x='{bar.x}' y='5' width='{bar.width}' height='60' fill='black'/>");s.Append($"<text x='{(x+12)/2}' y='83' text-anchor='middle' font-family='monospace' font-size='15'>{value.Trim('*')}</text></svg>");return s.ToString();
    }
}
