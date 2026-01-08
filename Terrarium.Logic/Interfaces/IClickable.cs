namespace Terrarium.Logic.Interfaces
{
    /// <summary>
    /// Interface for entities that can respond to click interactions.
    /// </summary>
    public interface IClickable
    {
        /// <summary>
        /// Called when the entity is clicked by the user.
        /// </summary>
        void OnClick();

        /// <summary>
        /// Determines if a point (x, y) is within the clickable area of this entity.
        /// </summary>
        bool ContainsPoint(double x, double y);
    }
}
