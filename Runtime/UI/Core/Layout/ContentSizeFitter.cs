using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Content Size Fitter", 141)]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// Resizes a RectTransform to fit the size of its content.
    /// </summary>
    /// <remarks>
    /// The ContentSizeFitter can be used on GameObjects that have one or more ILayoutElement components, such as Text, Image, HorizontalLayoutGroup, VerticalLayoutGroup, and GridLayoutGroup.
    /// </remarks>
    public class ContentSizeFitter : UIBehaviour, ILayoutSelfController
    {
        /// <summary>
        /// The size fit modes avaliable to use.
        /// </summary>
        public enum FitMode
        {
            /// <summary>
            /// Don't perform any resizing.
            /// </summary>
            Unconstrained,
            /// <summary>
            /// Resize to the minimum size of the content.
            /// </summary>
            MinSize,
            /// <summary>
            /// Resize to the preferred size of the content.
            /// </summary>
            PreferredSize,
            /// <summary>
            /// Resize to the maximum size of the content.
            /// </summary>
            MaxSize,
        }

        [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the width.
        /// </summary>
        public FitMode horizontalFit { get { return m_HorizontalFit; } set { if (SetPropertyUtility.SetStruct(ref m_HorizontalFit, value)) SetDirty(); } }

        [SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the height.
        /// </summary>
        public FitMode verticalFit { get { return m_VerticalFit; } set { if (SetPropertyUtility.SetStruct(ref m_VerticalFit, value)) SetDirty(); } }

        [SerializeField] protected RectTransform m_TargetRect;

        /// <summary>
        /// The RectTransform to consider as the target for the fitting.
        /// If none is set, the RectTransform of the component will be used.
        /// </summary>
        public RectTransform targetRect { get { return m_TargetRect != null ? m_TargetRect : m_Rect; } set { if (SetPropertyUtility.SetClass(ref m_TargetRect, value)) SetDirty(); } }

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        // field is never assigned warning
        #pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
        #pragma warning restore 649

        protected ContentSizeFitter()
        {}

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
            if (fitting == FitMode.Unconstrained)
            {
                // Keep a reference to the tracked transform, but don't control its properties:
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
                return;
            }

            m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            // Set size to min or preferred size
            if (fitting == FitMode.MinSize)
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(targetRect, axis));
            else if (fitting == FitMode.PreferredSize)
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(targetRect, axis));
            else
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, Mathf.Max(LayoutUtility.GetMaxSize(targetRect, axis), 0));
        }

        /// <summary>
        /// Calculate and apply the horizontal component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        /// Calculate and apply the vertical component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

    #endif
    }
}
