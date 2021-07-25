namespace Outhink.ResponseModels.CommandResponseModels
{
    /// <summary>
    /// Response model sent when a coin is inserted
    /// </summary>
    public class InsertCoinsResponseModel
    {
        /// <summary>
        /// Boolean flag to indicate if the coin insertion is done successfully
        /// </summary>
        public bool Succeeded { get; set; } = true;
    }
}
