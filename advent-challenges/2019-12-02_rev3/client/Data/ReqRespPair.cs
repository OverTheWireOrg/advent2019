
using System.Threading.Tasks;

class ReqRespPair
{
    public Hpmv.SessionRequest request;
    public TaskCompletionSource<Hpmv.SessionResponse> response;
}
