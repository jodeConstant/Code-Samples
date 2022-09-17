using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PikkuliHoiva
{
    [System.Serializable]
    public class SegmentPositionData
    {
        public RectTransform rectTransform;

        [HideInInspector] public Vector3 fromPosition;
        [HideInInspector] public Vector3 toPosition;

        public void LerpPosition(float fraction)
        {
            rectTransform.position = Vector3.Lerp(fromPosition, toPosition, fraction);
        }

        public SegmentPositionData()
        {
            rectTransform = null;
            fromPosition = Vector3.zero;
            toPosition = Vector3.zero;
        }
    }

    // Kuvastaa "kameran liikettä", kuvat liikkuvat vastakkaiseen suuntaan
    public enum SlideTransitionDirection { DOWN, LEFT, RIGHT, UP }

    public class SlideTransitionManager : MonoBehaviour
    {
        public Vector2 slideSeparationDistances;
        public bool saveValues = false;
        public float slideTransitionDelay = 0.5f;
        float extraDelay = 0f;
        public float ExtraDelay { set { extraDelay = value; }}
        public float slideChangeDuration = 1.3f;
        public Vector3 transitionVector;

        private SlideTransitionDirection setDirection;
        public SegmentPositionData lastSlide = new SegmentPositionData();
        public SegmentPositionData nextSlide = new SegmentPositionData();
        public SegmentPositionData segment_left_u = new SegmentPositionData();
        public SegmentPositionData segment_right_u = new SegmentPositionData();
        public SegmentPositionData segment_left_l = new SegmentPositionData();
        public SegmentPositionData segment_right_l = new SegmentPositionData();

        SlideTransitionDirection transitionDirection;
        public SlideTransitionDirection TransitionDirection
        {
            set { transitionDirection = value; }
        }

        public bool SegmentReferencesExist
        {
            get
            {
                return ((segment_left_u.rectTransform != null)
                && (segment_left_l.rectTransform != null)
                && (segment_right_u.rectTransform != null)
                && (segment_right_l.rectTransform != null));
            }
        }
        
        //saved positions
        public Vector3 segmentPosition_left_u;
        public Vector3 segmentPosition_right_u;
        public Vector3 segmentPosition_left_l;
        public Vector3 segmentPosition_right_l;

        public Vector2 wraparoundDistanceFractions = new Vector2(0.5f, 0.5f);
        
        public ComicPageEffectsManager effectsManager;

        bool canTransition = true;
        public bool CanTransition { get { return canTransition; } }

        public void StartTransition(RectTransform lastSlideTf, RectTransform nextSlideTf)
        {
            if (canTransition)
            {
                lastSlide.rectTransform = lastSlideTf;
                nextSlide.rectTransform = nextSlideTf;
                canTransition = false;
                StartCoroutine(SlideSwitchRoutine());
            }
        }
        
        void SwapComponentDataReferences(ref SegmentPositionData a, ref SegmentPositionData b)
        {
            SegmentPositionData swapVariable = a;
            a = b;
            b = swapVariable;
        }

        void LerpBackgroundComponents(float fraction)
        {
            segment_left_u.LerpPosition(fraction);
            segment_right_u.LerpPosition(fraction);
            
            segment_left_l.LerpPosition(fraction);
            segment_right_l.LerpPosition(fraction);
        }

        void WrapAroundComponents(SlideTransitionDirection direction)
        {
            if (direction == SlideTransitionDirection.DOWN)//moving upper parts down
            {
                //change positions, upper side components move down
                segment_left_u.fromPosition.y -= slideSeparationDistances.y * 2f;
                segment_right_u.fromPosition.y -= slideSeparationDistances.y * 2f;
                segment_left_u.toPosition.y -= slideSeparationDistances.y * 2f;
                segment_right_u.toPosition.y -= slideSeparationDistances.y * 2f;
                //swap references accordingly
                SwapComponentDataReferences(ref segment_left_l, ref segment_left_u);
                SwapComponentDataReferences(ref segment_right_l, ref segment_right_u);
            }
            else if (direction == SlideTransitionDirection.LEFT)//moving right parts left
            {
                //change positions, right side components move over to left edge
                segment_right_u.toPosition.x = segment_left_u.fromPosition.x;
                segment_right_l.toPosition.x = segment_left_l.fromPosition.x;
                segment_right_u.fromPosition.x = segment_left_u.fromPosition.x - transitionVector.x;
                segment_right_l.fromPosition.x = segment_left_l.fromPosition.x - transitionVector.x;
                //swap references accordingly
                SwapComponentDataReferences(ref segment_left_u, ref segment_right_u);
                SwapComponentDataReferences(ref segment_left_l, ref segment_right_l);
            }
            else if (direction == SlideTransitionDirection.RIGHT)//moving left parts right
            {
                //change positions, left side components move over to right edge
                segment_left_u.toPosition.x = segment_right_u.fromPosition.x;
                segment_left_l.toPosition.x = segment_right_l.fromPosition.x;
                segment_left_u.fromPosition.x = segment_right_u.fromPosition.x - transitionVector.x;
                segment_left_l.fromPosition.x = segment_right_l.fromPosition.x - transitionVector.x;
                //swap references accordingly
                SwapComponentDataReferences(ref segment_left_u, ref segment_right_u);
                SwapComponentDataReferences(ref segment_left_l, ref segment_right_l);
            }
            else//moving lower parts up
            {
                //change positions, lower side components move up
                segment_left_l.fromPosition.y += slideSeparationDistances.y * 2f;
                segment_left_l.toPosition.y += slideSeparationDistances.y * 2f;
                segment_right_l.fromPosition.y += slideSeparationDistances.y * 2f;
                segment_right_l.toPosition.y += slideSeparationDistances.y * 2f;
                //swap references accordingly
                SwapComponentDataReferences(ref segment_left_l, ref segment_left_u);
                SwapComponentDataReferences(ref segment_right_l, ref segment_right_u);
            }
        }

        void SetSlideSeparationDistances()
        {
            if (SegmentReferencesExist)
            {
                slideSeparationDistances
                = new Vector3(segment_right_u.rectTransform.position.x - segment_left_u.rectTransform.position.x, 
                                segment_left_u.rectTransform.position.y - segment_left_l.rectTransform.position.y);

                Debug.Log("FEELING REACTION MINIGAME: Setup of slide separation distances: x is: " 
                + slideSeparationDistances.x 
                + ", y is: " 
                + slideSeparationDistances.y);
            }
        }

        void SaveCurrentSegmentPositions()
        {
            if (SegmentReferencesExist)
            {
                segmentPosition_left_u = segment_left_u.rectTransform.position;
                segmentPosition_left_l = segment_left_l.rectTransform.position;
                segmentPosition_right_u = segment_right_u.rectTransform.position;
                segmentPosition_right_l = segment_right_l.rectTransform.position;
            }
            else
            {
                Debug.LogWarning("FEELING REACTION MINIGAME: " + name + ": Required fields are not set!");
            }
        }

        //
        void InitializeTransitions(SlideTransitionDirection direction)//0 = down, 1 = right, 2 = left, 3+ = up
        {
            if ((lastSlide.rectTransform != null)
                && (nextSlide.rectTransform != null)
                && SegmentReferencesExist)
            {
                if (direction == SlideTransitionDirection.DOWN) { transitionVector = new Vector3(0f, slideSeparationDistances.y, 0f); }
                else if (direction == SlideTransitionDirection.LEFT) { transitionVector = new Vector3(slideSeparationDistances.x, 0f, 0f); }
                else if (direction == SlideTransitionDirection.RIGHT) { transitionVector = new Vector3(-slideSeparationDistances.x, 0f, 0f); }
                else { transitionVector = new Vector3(0f, -slideSeparationDistances.y, 0f); }

                //from
                lastSlide.fromPosition = lastSlide.rectTransform.position;
                nextSlide.fromPosition = lastSlide.fromPosition - transitionVector;
                segment_left_u.fromPosition = segmentPosition_left_u;
                segment_right_u.fromPosition = segmentPosition_right_u;
                segment_left_l.fromPosition = segmentPosition_left_l;
                segment_right_l.fromPosition = segmentPosition_right_l;

                //to
                lastSlide.toPosition = lastSlide.fromPosition + transitionVector;
                nextSlide.toPosition = lastSlide.fromPosition;
                segment_left_u.toPosition = segment_left_u.fromPosition + transitionVector;
                segment_right_u.toPosition = segment_right_u.fromPosition + transitionVector;
                segment_left_l.toPosition = segment_left_l.fromPosition + transitionVector;
                segment_right_l.toPosition = segment_right_l.fromPosition + transitionVector;
            }
            else
            {
                Debug.LogWarning("FEELING REACTION MINIGAME: " + name + ": Required fields are not set!");
            }
        }


        //0 = down, 1 = right, 2 = left, 3+ = up
        IEnumerator SlideSwitchRoutine()
        {
            effectsManager.gameObject.SetActive(false);

            if (slideTransitionDelay > 0f) { yield return new WaitForSeconds(slideTransitionDelay); }
            else { yield return new WaitForEndOfFrame(); }// make sure the pressed button reference gets assigned BEFORE the rest of routine
            
            //extra delay
            if (extraDelay > 0f) { yield return new WaitForSeconds(extraDelay); extraDelay = 0f; }

            //save direction for the duration of the routine
            SlideTransitionDirection usedDirection = transitionDirection;

            InitializeTransitions(usedDirection);

            float transitionTimer = 0f;
            float wraparoundTime = slideChangeDuration * 
                ((usedDirection == SlideTransitionDirection.LEFT || usedDirection == SlideTransitionDirection.RIGHT) ? 
                    wraparoundDistanceFractions.x : wraparoundDistanceFractions.y);
            nextSlide.rectTransform.gameObject.SetActive(true);
            
            while (transitionTimer < wraparoundTime)
            {
                lastSlide.LerpPosition(transitionTimer / slideChangeDuration);
                nextSlide.LerpPosition(transitionTimer / slideChangeDuration);
                LerpBackgroundComponents(transitionTimer / slideChangeDuration);

                transitionTimer += Time.deltaTime;
                yield return null;
            }

            //swap segments' positions as needed
            WrapAroundComponents(usedDirection);
            Debug.Log("FEELING REACTION MINIGAME: Slide change routine: component positions swapped!");

            //continue lerp
            while (transitionTimer < slideChangeDuration)
            {
                lastSlide.LerpPosition(transitionTimer / slideChangeDuration);
                nextSlide.LerpPosition(transitionTimer / slideChangeDuration);
                LerpBackgroundComponents(transitionTimer / slideChangeDuration);

                transitionTimer += Time.deltaTime;
                yield return null;
            }

            // finalize positions, might be redundant
            lastSlide.LerpPosition(1.0f);
            nextSlide.LerpPosition(1.0f);
            LerpBackgroundComponents(1.0f);

            lastSlide.rectTransform.gameObject.SetActive(false);

            effectsManager.gameObject.SetActive(true);

            canTransition = true;
            transitionDirection = SlideTransitionDirection.DOWN;
            Debug.Log("FEELING REACTION MINIGAME: Slide change routine ended!");
        }

        void Start()
        {
            SetSlideSeparationDistances();
            SaveCurrentSegmentPositions();
        }

        void OnValidate()
        {
            if (saveValues)
            {
                SetSlideSeparationDistances();
                SaveCurrentSegmentPositions();
                saveValues = false;
            }
        }
    }
}