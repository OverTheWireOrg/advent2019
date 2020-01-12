using Microsoft.AspNetCore.Components;

namespace client.Pages {
    public partial class Dialog {
        [Parameter]
        public RenderFragment ChildContent {get; set; }

        [Parameter]
        public EventCallback OnDismiss {get; set;}

        [Parameter]
        public bool Visible {get; set;}
    }
}