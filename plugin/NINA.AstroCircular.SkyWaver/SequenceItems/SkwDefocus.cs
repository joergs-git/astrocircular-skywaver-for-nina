using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.AstroCircular.SkyWaver.SequenceItems {

    /// <summary>
    /// Moves the focuser by a configurable number of steps for SKW collimation.
    /// Stores the original position so the orchestrator can return to focus later.
    /// </summary>
    [ExportMetadata("Name", "SKW Defocus")]
    [ExportMetadata("Description", "Move focuser by N steps for SkyWave collimation")]
    [ExportMetadata("Icon", "FocusSVG")]
    [ExportMetadata("Category", "SkyWave Collimation")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SkwDefocus : SequenceItem {
        private readonly IFocuserMediator focuserMediator;

        [JsonProperty]
        public int DefocusSteps { get; set; } = 2442;

        [JsonProperty]
        public int DefocusDirection { get; set; } = 1; // +1 extra-focal, -1 intra-focal

        /// <summary>Stores focuser position before defocus for later refocus.</summary>
        public int OriginalPosition { get; set; } = -1;

        public string DirectionLabel => DefocusDirection > 0 ? "Extra-focal (+)" : "Intra-focal (-)";

        [ImportingConstructor]
        public SkwDefocus(IFocuserMediator focuserMediator) {
            this.focuserMediator = focuserMediator;
        }

        // Private constructor for cloning
        private SkwDefocus(SkwDefocus cloneMe) : this(cloneMe.focuserMediator) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new SkwDefocus(this) {
                DefocusSteps = DefocusSteps,
                DefocusDirection = DefocusDirection
            };
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            // Store original position before moving
            var focuserInfo = focuserMediator.GetInfo();
            if (!focuserInfo.Connected) {
                throw new SequenceEntityFailedException("Focuser is not connected. Connect the focuser before running SKW collimation.");
            }

            OriginalPosition = focuserInfo.Position;
            int relativeMove = DefocusSteps * DefocusDirection;

            progress?.Report(new ApplicationStatus {
                Status = $"SKW: Moving focuser {(relativeMove > 0 ? "+" : "")}{relativeMove} steps"
            });

            await focuserMediator.MoveFocuserRelative(relativeMove, ct);

            progress?.Report(new ApplicationStatus {
                Status = $"SKW: Focuser moved to {focuserMediator.GetInfo().Position} (was {OriginalPosition})"
            });
        }

        public override string ToString() {
            return $"Category: SkyWave Collimation, Item: SkwDefocus, Steps: {DefocusSteps}, Direction: {DirectionLabel}";
        }
    }
}
