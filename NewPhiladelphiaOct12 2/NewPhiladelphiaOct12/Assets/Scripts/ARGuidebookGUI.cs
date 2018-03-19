using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vuforia;
using UnityEngine.SceneManagement;

public enum FocusedPOIMode
{
    Center,
    Recent
}

public class ARGuidebookGUI : MonoBehaviour
{
    private enum GUIPage
    {
        None,
        Instructions,
        TrackingLost,
        Debug,
        Camera
    }

    // Unity Variables
    public TargetSelector Selector;

    public POIType MinSelectedPOIType = POIType.Additional;
    public FocusedPOIMode FocusedPOIMode = FocusedPOIMode.Center;

    public Texture2D TrackingBoxTexture;
    public Transform[] DebugTransforms;

    public static bool playAnimatedTitle = true;
    public Texture2D[] animatedTitle;
    public Texture2D animatedTitleBackground;
    private int framePointer = 0;
    private int timer;
    public static float playRate = 5.0f;

    public GUISkin mySkin;
    public Texture titleTexture;
    public Texture homeButtonTexture;
    public Texture vetButtonTexture;
    public Texture arrowTexture;
    public Texture slateTexture;
    public Texture slateUnfoundTexture;
    public Texture vetButtonUnfoundTexture;
    public Texture thimbleTexture;
    public Texture thimbleUnfoundTexture;
    public Texture trivetTexture;
    public Texture trivetUnfoundTexture;


    public static bool displayTargetButton = false;
    public static bool displayArtifactPrompt = false;
    public static bool buttonFound = false;
    public static bool slateFound = false;
    public static bool thimbleFound = false;
    public static bool trivetFound = false;
    private float messageTimer = 8.0f;

    public static float mainMessageTimer = 20.0f;
    public static bool displayMessage = false;
    public static string messageText;
    public static string promptText;
    public static Texture messageImage;
    public static bool displayImage = false;

    // Fields
    private GUIPage basePage = GUIPage.None;
    private List<GUIPage> menuStack = new List<GUIPage>();
    private GUIStyle trackingBoxStyle = new GUIStyle();

    private string bannerText = "";

    private TargetAnnotation selectedTarget = null;
    private List<PointOfInterest> selectedPOIs = new List<PointOfInterest>();
    private PointOfInterest focusedPOI = null;

    private float backEventTime;
    private bool backEventDispatched = false;
    private bool guiInput = false;
    private bool waitingForSecondTap;
    private Vector3 firstTapPosition;
    private float firstTapTime;
    private const float MaxTapTime = .5f;
    private const float MaxTapDistance = .1f; // screen space

    private CameraDevice.CameraDeviceMode cameraDeviceMode = CameraDevice.CameraDeviceMode.MODE_DEFAULT;
    private bool autoFocus = true;

    // Properties
    private VuforiaBehaviour ARCamera
    {
        get { return VuforiaBehaviour.Instance; }
    }

    private bool InMenu
    {
        get { return menuStack.Count > 0; }
    }
    private GUIPage Page
    {
        get
        {
            if (InMenu)
                return menuStack[menuStack.Count - 1];
            else
                return basePage;
        }
        set { basePage = value; }
    }
    private bool GUIShowing
    {
        get { return Page != GUIPage.None; }
    }

    // Unity Events
    void Start()
    {
        if (ARCamera != null)
            //cameraDeviceMode = ARCamera.CameraDeviceMode;
            CameraDevice.Instance.SetFocusMode(autoFocus ? CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO : CameraDevice.FocusMode.FOCUS_MODE_NORMAL);

        if (Selector != null)
            Selector.OnSelectedTrackableChanged += SelectedTrackableChanged;

        Page = GUIPage.Instructions;
        bannerText = "";
        trackingBoxStyle.normal.background = TrackingBoxTexture;
    }

    void Update()
    {

        UpdatePOIs();

        //This cycles through animated title frames in a loop. Note if you add a -1 after animatedTitle.Length it will not loop.
        if (playAnimatedTitle)
        {
            timer++;
            if ((timer >= playRate) && framePointer < animatedTitle.Length)
            {
                framePointer++;
                //mainTexture = animatedTitle[framePointer];
                timer = 0;

                if (framePointer >= 30)
                {
                    framePointer = 0;
                }
            }
        }

        if (Time.time - backEventTime > 1.0f)
            backEventDispatched = false;

        if (Input.GetKey(KeyCode.Escape) && !backEventDispatched)
        {
            OnBack();

            backEventDispatched = true;
            backEventTime = Time.time;
        }

        if (!guiInput)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (waitingForSecondTap)
                {
                    // check if time and position match:
                    int smallerScreenDimension = Screen.width < Screen.height ? Screen.width : Screen.height;
                    if (Time.time - firstTapTime < MaxTapTime &&
                        Vector3.Distance(Input.mousePosition, firstTapPosition) < smallerScreenDimension * MaxTapDistance)
                    {
                        // it's a double tap
                        OnDoubleTap();

                        waitingForSecondTap = false;
                    }
                    else
                    {
                        // too late/far to be a double tap, treat it as a new first tap
                        OnTap(); // for original first tap

                        firstTapPosition = Input.mousePosition;
                        firstTapTime = Time.time;
                    }
                }
                else
                {
                    // it's the first tap
                    waitingForSecondTap = true;
                    firstTapPosition = Input.mousePosition;
                    firstTapTime = Time.time;
                }
            }
            else
            {
                // time window for second tap has passed, trigger single tap
                if (waitingForSecondTap && Time.time - firstTapTime > MaxTapTime)
                {
                    OnTap();

                    waitingForSecondTap = false;
                }
            }
        }
        else
        {
            guiInput = false;
        }
    }

    void OnGUI()
    {
        //Sets GuiSkin for my Gui objects
        GUI.skin = mySkin;

        //This sets the font size of the courtesy text
        GUIStyle labelCourtesy = GUI.skin.GetStyle("courtesy");
        labelCourtesy.fontSize = Screen.width / 70;

        //This sets the font size of the message text
        GUIStyle labelMessages = GUI.skin.GetStyle("messages");
        labelMessages.fontSize = Screen.width / 50;

        //This sets the font size of the prompt text
        GUIStyle promptMessages = GUI.skin.GetStyle("prompts");
        promptMessages.fontSize = Screen.width / 50;

        //This sets the font size of the itemsLabel text
        GUIStyle labelItems = GUI.skin.GetStyle("itemsLabel");
        labelItems.fontSize = Screen.width / 62;

        //This sets the GUI label font size and scales according to the Screen size
        mySkin.label.fontSize = Screen.width / 45;
        BannerGUI();

        //This checks to see if the artifact prompt should be displayed
        if (displayArtifactPrompt)
        {
            mainMessageTimer -= Time.deltaTime;
            if (displayArtifactPrompt && mainMessageTimer < 35)
            {
                //This displays a message
                GUI.Box(new Rect(0, Screen.height - Screen.height / 6.6f, Screen.width, Screen.height / 10), messageText, "prompts");


                GUI.DrawTexture(new Rect(Screen.width - Screen.height / 2.8f, Screen.height - Screen.height / 2.8f, Screen.height / 4, Screen.height / 4), arrowTexture);
                messageText = "When viewing the Guideposts, see if you can find these artifacts. Tap on them to add them to your items.";
            }
            if (mainMessageTimer <= 0)
            {
                displayMessage = false;
                displayImage = false;
                displayArtifactPrompt = false;
            }
        }

        //This checks to see if a message should be displayed
        if (displayMessage)
        {
            mainMessageTimer -= Time.deltaTime;
            if (mainMessageTimer > 0)
            {
                //This displays a message
                GUI.Box(new Rect(0, Screen.height - Screen.height / 6.6f, Screen.width, Screen.height / 10), messageText, "messages");


                if (displayImage)
                {
                    if (GUI.Button(new Rect(Screen.width / 2 - Screen.height / 1.6f / 2, Screen.height / 6, Screen.height / 1.6f, Screen.height / 1.6f), messageImage, "imagesBox"))
                    {
                        displayImage = false;
                    }
                }


            }
            if (mainMessageTimer <= 0)
            {
                displayMessage = false;
                displayImage = false;
                displayArtifactPrompt = false;
            }
        }


        switch (Page)
        {
            case GUIPage.Instructions:
                IntroGUI();
                break;
            case GUIPage.TrackingLost:
                TrackingLostGUI();
                break;
            case GUIPage.Debug:
                DebugGUI();
                break;
            case GUIPage.Camera:
                CameraGUI();
                break;
        }
    }

    // Delegates
    void SelectedTrackableChanged(TrackableBehaviour previous, TrackableBehaviour selected)
    {
        if (selected != null)
        {
            StopCoroutine("OnTrackingLost");
            Page = GUIPage.None;
            bannerText = selected.TrackableName; // default in case no TargetAnnotation component
            SetSelectedTarget(Selector.SelectedTarget);
        }
        else if (previous != null)
        {
            StartCoroutine("OnTrackingLost");
        }
    }

    // Methods
    /*** Put code here when a new guidepost becomes actived ***/
    private void TargetActivated(TargetAnnotation target)
    {
        bannerText = target.Title;

        //This ensures that the Target Box button reappears when a target is recognized
        displayTargetButton = true;

    }

    /*** Put code here when a guidepost becomes deactived ***/
    private void TargetDeactivated(TargetAnnotation target)
    {
    }

    /*** Put code here when a point of interest comes into view ***/
    private void POIActivated(PointOfInterest poi)
    {
    }

    /*** Put code here when a point of interest leave the view or is hidden ***/
    private void POIDeactivated(PointOfInterest poi)
    {
    }

    /*** Put code here when a new point of interest becomes focused ***/
    private void POIFocused(PointOfInterest poi)
    {
    }

    /*** Put code here when a point of interest becomes unfocused ***/
    private void POIUnfocused(PointOfInterest poi)
    {
    }

    private void SetSelectedTarget(TargetAnnotation newTarget)
    {
        if (selectedTarget != null)
        {
            selectedTarget.Deactivate();
            TargetDeactivated(selectedTarget);
        }
        selectedTarget = newTarget;
        if (selectedTarget != null)
        {
            selectedTarget.Activate();
            TargetActivated(selectedTarget);
        }
        UpdatePOIs();
    }

    private IEnumerator OnTrackingLost()
    {
        //This ensures that the target box button disappears
        displayTargetButton = false;

        yield return new WaitForSeconds(1.0f);
        Page = GUIPage.TrackingLost;
        bannerText = "Tracking was lost. Please re-center one of the Virtual Guideposts\u2122 in the box.";
        SetSelectedTarget(null);

    }

    private bool IsSelectedPOI(PointOfInterest poi)
    {
        return poi.Type >= MinSelectedPOIType && poi.IsOnScreen;
    }

    private void UpdatePOIs()
    {
        // remove any that aren't selected
        for (int i = 0; i < selectedPOIs.Count; i++)
        {
            var poi = selectedPOIs[i];

            if (!IsSelectedPOI(poi))
            {
                if (poi == focusedPOI)
                {
                    focusedPOI.Unfocus();
                    POIUnfocused(focusedPOI);
                    focusedPOI = null;
                }
                selectedPOIs.Remove(poi);
                poi.Deactivate();
                POIDeactivated(poi);
            }
        }

        //foreach (var poi in selectedPOIs)
        //{
        //    if (!IsSelectedPOI(poi))
        //    {
        //        if (poi == focusedPOI)
        //        {
        //            focusedPOI.Unfocus();
        //            POIUnfocused(focusedPOI);
        //            focusedPOI = null;
        //        }
        //        selectedPOIs.Remove(poi);
        //        poi.Deactivate();
        //        POIDeactivated(poi);
        //    }
        //}

        PointOfInterest newFocus = focusedPOI;

        PointOfInterest[] pois = GetComponentsInChildren<PointOfInterest>();


        for (int i = 0; i < pois.Length; i++)
        {
            var poi = pois[i];

            if (!selectedPOIs.Contains(poi) && IsSelectedPOI(poi))
            {
                selectedPOIs.Add(poi);
                poi.Activate();
                POIActivated(poi);
                if (FocusedPOIMode == FocusedPOIMode.Recent)
                {
                    newFocus = poi;
                }
            }
        }

        //foreach (PointOfInterest poi in pois)
        //{
        //    if (!selectedPOIs.Contains(poi) && IsSelectedPOI(poi))
        //    {
        //        selectedPOIs.Add(poi);
        //        poi.Activate();
        //        POIActivated(poi);
        //        if (FocusedPOIMode == FocusedPOIMode.Recent)
        //        {
        //            newFocus = poi;
        //        }
        //    }
        //}

        if (FocusedPOIMode == FocusedPOIMode.Center)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            float closestDistance = float.PositiveInfinity;
            foreach (PointOfInterest poi in selectedPOIs)
            {
                Vector3 screenPoint = poi.ScreenPoint;
                screenPoint -= screenCenter;
                screenPoint.z = 0f;
                if (screenPoint.sqrMagnitude < closestDistance)
                {
                    closestDistance = screenPoint.sqrMagnitude;
                    newFocus = poi;
                }
            }
        }

        if (newFocus != focusedPOI)
        {
            if (focusedPOI != null)
            {
                focusedPOI.Unfocus();
                POIUnfocused(focusedPOI);
            }
            focusedPOI = newFocus;
            if (focusedPOI != null)
            {
                focusedPOI.Focus();
                POIFocused(focusedPOI);
            }
        }
    }

    private void BannerGUI()
    {


        if (bannerText != "")
        {
            //This controls the size and position of the top most GUI Box that says To get Started....
            Rect rect = new Rect(0, 0, Screen.width, Screen.height / 10);
            GUILayout.BeginArea(rect, GUI.skin.box);

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label(bannerText, centeredStyle);

            GUILayout.EndArea();

            //Restart Button
            if (GUI.Button(new Rect(20, 5, Screen.height / 10, Screen.height / 10), homeButtonTexture, "inventoryButton"))
            {
                displayArtifactPrompt = false;
                displayMessage = false;
                playAnimatedTitle = true;
                displayImage = false;
                trivetFound = false;
                buttonFound = false;
                thimbleFound = false;
                slateFound = false;
                messageText = "";
                messageTimer = 8.0f;
                //Application.LoadLevel(0);
                SceneManager.LoadScene(0);

            }

            //This determines whether or not to display the Target Button on upper right side of screen
            if (displayTargetButton)
            {

                //Reset Target Box Button
                if (GUI.Button(new Rect(Screen.width - Screen.height / 8, 10, Screen.height / 12, Screen.height / 12), "", "homeButton"))
                {

                    displayMessage = false;
                    displayImage = false;
                    //Put home code here

                    Page = GUIPage.TrackingLost;
                    bannerText = "Please re-center one of the Virtual Guideposts\u2122 in the box.";

                    SetSelectedTarget(null);
                    selectedTarget.Deactivate();
                    TargetDeactivated(selectedTarget);
                    displayTargetButton = false;

                }
                //This displays the reset target text
                GUI.Label(new Rect(Screen.width - Screen.height / 6, Screen.height / 100.0f, Screen.height / 6, Screen.height / 10), "reset", "itemsLabel");

            }

            //This draws the items label above the artifacts
            //GUI.Label(new Rect(Screen.width-Screen.height/6,Screen.height/14.2f, Screen.height/6, Screen.height/10), "artifacts", "itemsLabel");


            //This draws the Thimble Inventory icon
            if (thimbleFound == false)
            {
                if (GUI.Button(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 1.18f, Screen.height / 6, Screen.height / 6), thimbleUnfoundTexture, "inventoryButton"))
                {
                    displayMessage = true;
                    displayArtifactPrompt = false;
                    mainMessageTimer = 20.0f;
                    messageText = "hint: find the artifact used for sewing.";
                }
            }
            else if (thimbleFound)
            {
                GUI.DrawTexture(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 1.18f, Screen.height / 6, Screen.height / 6), thimbleTexture);
            }



            //This draws the slate Inventory icon
            if (slateFound == false)
            {
                if (GUI.Button(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 1.48f, Screen.height / 6, Screen.height / 6), slateUnfoundTexture, "inventoryButton"))
                {
                    displayMessage = true;
                    displayArtifactPrompt = false;
                    mainMessageTimer = 20.0f;
                    messageText = "hint: find the artifact used by school children.";
                }
            }
            else if (slateFound)
            {
                GUI.DrawTexture(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 1.48f, Screen.height / 6, Screen.height / 6), slateTexture);
            }



            //This draws the Vet Button Inventory icon
            if (buttonFound == false)
            {
                if (GUI.Button(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 1.985f, Screen.height / 6, Screen.height / 6), vetButtonUnfoundTexture, "inventoryButton"))
                {
                    displayMessage = true;
                    displayArtifactPrompt = false;
                    mainMessageTimer = 20.0f;
                    messageText = "hint: find the artifact from a Union soldier's uniform.";
                }
            }
            else if (buttonFound)
            {
                GUI.DrawTexture(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 1.985f, Screen.height / 6, Screen.height / 6), vetButtonTexture);
            }

            //This draws the trivet Inventory icon
            if (trivetFound == false)
            {
                if (GUI.Button(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 3.0f, Screen.height / 6, Screen.height / 6), trivetUnfoundTexture, "inventoryButton"))
                {
                    displayMessage = true;
                    displayArtifactPrompt = false;
                    mainMessageTimer = 20.0f;
                    messageText = "hint: find the artifact likely made by a New Philadelphia blacksmith.";
                }
            }
            else if (trivetFound)
            {
                GUI.DrawTexture(new Rect(Screen.width - Screen.height / 6, Screen.height - Screen.height / 3.0f, Screen.height / 6, Screen.height / 6), trivetTexture);
            }


            //This checks to see if the user has found all of the items and if so sends a congratulatory message
            if (thimbleFound == true && buttonFound == true && slateFound == true && trivetFound == true)
            {

                if (messageTimer > 0)
                {
                    messageTimer -= Time.deltaTime;
                    //This displays the Congratulations Message
                    GUI.Label(new Rect(Screen.width - Screen.height / 1.8f, Screen.height / 3, Screen.height / 3, Screen.height / 4), "Great job! You have found all of the artifacts.", "messages");

                }




            }
        }
    }


    private void IntroGUI()
    {



        //This displays title image
        //GUI.Box(new Rect(15, Screen.height-50, Screen.height, Screen.height/20), "Funded by the National Park Service Underground Railroad Network to Freedom and the Illinois Rural Electric Cooperative", "courtesy");

        //credits Button
        if (GUI.Button(new Rect(Screen.width - Screen.height / 3, Screen.height - Screen.height / 4, Screen.height / 4, Screen.height / 4), "", "infoButton"))
        {
            displayTargetButton = false;
            //Put help code here
            //Application.LoadLevel(1);
            SceneManager.LoadScene(1);
        }

        GUI.DrawTexture(new Rect(Screen.width / 2 - Screen.height / 2, Screen.height / 128, Screen.height, Screen.height), animatedTitleBackground);
        GUI.DrawTexture(new Rect(Screen.width / 2 - Screen.height / 2, Screen.height / 128, Screen.height, Screen.height), animatedTitle[framePointer]);


        //Start Button
        if (GUI.Button(new Rect(Screen.width / 2 - Screen.height / 6, Screen.height - Screen.height / 2.8f, Screen.height / 3, Screen.height / 3), ""))
        {
            Page = GUIPage.TrackingLost;
            bannerText = "To get started, center one of the Virtual Guideposts\u2122 in the box.";
            playAnimatedTitle = false;
            guiInput = true;

            //temporary prompt to find the artifacts
            displayArtifactPrompt = true;

            mainMessageTimer = 40.0f;


        }


        Rect rect = new Rect();
        if (Screen.width < Screen.height)
        {
            rect.width = Screen.width / 4;
            rect.height = Screen.height * .5f;
        }
        else
        {
            //the float determines the width of the Intro GUI box. It was originally .8
            rect.width = Screen.width * .6f;
            rect.height = Screen.height * .22f;
        }
        rect.x = (Screen.width - rect.width) / 2f;
        rect.y = (Screen.height + Screen.height / 2) / 2f;

        //GUILayout.BeginArea(rect,GUI.skin.box);

        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;

        //This is the original intro Gui
        //GUILayout.Label("Welcome to the New Philadelphia Augmented Reality Guidebook.",centeredStyle);

        GUILayout.FlexibleSpace();

        //if (GUILayout.Button("start")) {
        //Page = GUIPage.TrackingLost;
        //bannerText = "To get started, center one of the Virtual Guideposts\u2122 in the box.";

        //guiInput = true;
        //}

        //GUILayout.EndArea();
    }

    private void TrackingLostGUI()
    {
        displayTargetButton = false;

        Rect rect = new Rect();
        rect.width = rect.height = Mathf.Min(Screen.width, Screen.height) * .7f;
        rect.x = (Screen.width - rect.width) / 2f;
        rect.y = (Screen.height - rect.height) / 2f;

        GUILayout.BeginArea(rect, trackingBoxStyle);
        GUILayout.EndArea();
    }

    private void DebugGUI()
    {
        Rect rect = new Rect();
        if (Screen.width < Screen.height)
        {
            float border = Screen.width * .2f;
            rect.width = Screen.width - border;
            rect.height = Screen.height - border;
        }
        else
        {
            rect.height = Screen.height * .8f;
            rect.width = Screen.width * .5f;
        }
        rect.x = (Screen.width - rect.width) / 2f;
        rect.y = (Screen.height - rect.height) / 2f;

        GUILayout.BeginArea(rect, GUI.skin.box);

        HideUntracked hideUntracked = GetComponent<HideUntracked>();

        // Options
        if (Selector is VuforiaTargetSelector)
        {
            VuforiaTargetSelector vuforiaTracker = Selector as VuforiaTargetSelector;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Trackable Type");
            vuforiaTracker.SetTrackableType(EnumToolbar(vuforiaTracker.TrackableType));
            GUILayout.EndHorizontal();
            if (vuforiaTracker.TrackableType == TrackableType.ImageTarget)
            {
                vuforiaTracker.SetExtendedTracking(GUILayout.Toggle(vuforiaTracker.UseExtendedTracking, "Extending Tracking"));
                vuforiaTracker.SetPersistExtendedTracking(GUILayout.Toggle(vuforiaTracker.PersistExtendedTracking, "Persist Extended Tracking"));
            }
        }
        if (Selector is InertialTracking)
        {
            InertialTracking inertialTracker = Selector as InertialTracking;
            if (SystemInfo.supportsGyroscope)
                inertialTracker.UseInertialTracking = GUILayout.Toggle(inertialTracker.UseInertialTracking, "Inertial Tracking");
        }

        if (GUI.changed)
            guiInput = true;

        if (hideUntracked != null)
        {
            GUI.changed = false;
            hideUntracked.HideTargets = GUILayout.Toggle(hideUntracked.HideTargets, "Hide Targets");
            hideUntracked.HideDebug = GUILayout.Toggle(hideUntracked.HideDebug, "Hide Debug");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Depth Masks");
            hideUntracked.DepthMaskMode = EnumToolbar(hideUntracked.DepthMaskMode);
            GUILayout.EndHorizontal();
            hideUntracked.SelectedPOIsOnly = GUILayout.Toggle(hideUntracked.SelectedPOIsOnly, "Selected POIs Only");
            if (GUI.changed)
            {
                hideUntracked.UpdateShowing();

                guiInput = true;
            }
        }

        GUILayout.Space(5);

        // Status
        if (Selector != null)
        {
            if (Selector.SelectedTarget != null)
                GUILayout.Label("Active: " + Selector.SelectedTrackable.TrackableName + " [" + Selector.SelectedTrackable.CurrentStatus + "]");
            else
                GUILayout.Label("Active: (none)");

            /*InertialTracking inertialTracker = Selector as InertialTracking;
			if (inertialTracker != null && SystemInfo.supportsGyroscope) {
				GUILayout.Label("Acceleration: " + Input.gyro.userAcceleration * 9.81f);
			}*/
        }

        foreach (TrackableBehaviour trackable in VuforiaTargetSelector.Trackables)
        {
            GUILayout.Label(trackable.TrackableName + "[" + trackable.CurrentStatus + "]");
        }

        GUILayout.Space(10);

        foreach (Transform trans in DebugTransforms)
        {
            string label = trans.gameObject.name + ": " + trans.position;
            GUILayout.Label(label);
        }

        GUILayout.Space(10);

        if (SceneManager.sceneCountInBuildSettings > 1)
        {
            string[] names = new string[SceneManager.sceneCountInBuildSettings];
            for (int i = 0; i < names.Length; ++i)
            {
                names[i] = string.Format("Scene {0}", i + 1);
            }

            int level = GUILayout.Toolbar(SceneManager.sceneCountInBuildSettings, names);
            if (level != SceneManager.sceneCountInBuildSettings)
            {
             
                SceneManager.LoadScene(level);
                guiInput = true;
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Camera Settings"))
        {
            PopMenu();
            PushMenu(GUIPage.Camera);

            guiInput = true;
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Close"))
        {
            PopMenu();

            guiInput = true;
        }

        GUILayout.EndArea();
    }

    private void CameraGUI()
    {
        Rect rect = new Rect();
        if (Screen.width < Screen.height)
        {
            float border = Screen.width * .2f;
            rect.width = Screen.width - border;
            rect.height = Screen.height - border;
        }
        else
        {
            rect.height = Screen.height * .8f;
            rect.width = Screen.width * .5f;
        }
        rect.x = (Screen.width - rect.width) / 2f;
        rect.y = (Screen.height - rect.height) / 2f;

        GUILayout.BeginArea(rect, GUI.skin.box);

        GUI.changed = false;

        GUIStyle headingStyle = new GUIStyle(GUI.skin.label);
        headingStyle.alignment = TextAnchor.MiddleCenter;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Camera Direction", headingStyle);
        CameraDevice.CameraDirection camDir = EnumToolbar(CameraDevice.Instance.GetCameraDirection(), new string[] { "Back", "Front" }, new CameraDevice.CameraDirection[] { CameraDevice.CameraDirection.CAMERA_BACK, CameraDevice.CameraDirection.CAMERA_FRONT }, ToolbarStyle.Horizontal);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Camera Mode", headingStyle);
        cameraDeviceMode = EnumToolbar(cameraDeviceMode, new string[] { "Default", "Speed", "Quality" }, new CameraDevice.CameraDeviceMode[] { CameraDevice.CameraDeviceMode.MODE_DEFAULT, CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_SPEED, CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY }, ToolbarStyle.Horizontal);
        GUILayout.EndVertical();

        if (GUI.changed)
        {
            guiInput = true;

            CameraDevice.Instance.Stop();
            CameraDevice.Instance.Deinit();
            CameraDevice.Instance.Init(camDir);
            CameraDevice.Instance.SelectVideoMode(cameraDeviceMode);
            CameraDevice.Instance.Start();
        }

        GUI.changed = false;
        autoFocus = GUILayout.Toggle(autoFocus, "Auto Focus");
        if (GUI.changed)
        {
            guiInput = true;

            CameraDevice.Instance.SetFocusMode(autoFocus ? CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO : CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
        }
        if (!autoFocus && GUILayout.Button("Focus"))
        {
            guiInput = true;

            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
        }

        GUILayout.Space(5);

        CameraDevice.VideoModeData modeData = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_DEFAULT);
        GUILayout.Label("Camera mode (default): " + modeData.width + " x " + modeData.height + ", " + modeData.frameRate + " fps");
        modeData = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_SPEED);
        GUILayout.Label("Camera mode (speed): " + modeData.width + " x " + modeData.height + ", " + modeData.frameRate + " fps");
        modeData = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY);
        GUILayout.Label("Camera mode (quality): " + modeData.width + " x " + modeData.height + ", " + modeData.frameRate + " fps");

        GUILayout.Space(5);

        VuforiaRenderer.VideoTextureInfo texInfo = VuforiaRenderer.Instance.GetVideoTextureInfo();
        GUILayout.Label("texture size: (" + texInfo.textureSize.x + " x " + texInfo.textureSize.y + ")");
        GUILayout.Label("image size: (" + texInfo.imageSize.x + " x " + texInfo.imageSize.y + ")");

        VuforiaRenderer.VideoBGCfgData bgInfo = VuforiaRenderer.Instance.GetVideoBackgroundConfig();
        GUILayout.Label("position: (" + bgInfo.position.x + ", " + bgInfo.position.y + ")");
        GUILayout.Label("size: (" + bgInfo.size.x + " x " + bgInfo.size.y + ")");

        if (ARCamera != null)
            GUILayout.Label("Viewport: ");

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Close"))
        {
            PopMenu();

            guiInput = true;
        }

        GUILayout.EndArea();
    }

    private void OnBack()
    {
        if (!PopMenu())
            Application.Quit();
    }

    private void OnTap()
    {
    }

    private void OnDoubleTap()
    {
        if (Debug.isDebugBuild && !InMenu)
        {
            //Page = GUIPage.None;
            PushMenu(GUIPage.Debug);
        }
        else if (InMenu)
        {
            ClearMenus();
        }
    }

    private void PushMenu(GUIPage page)
    {
        menuStack.Add(page);
    }

    private bool PopMenu()
    {
        if (InMenu)
        {
            menuStack.RemoveAt(menuStack.Count - 1);
            return true;
        }
        return false;
    }

    private void ClearMenus()
    {
        menuStack.Clear();
    }

    private enum ToolbarStyle
    {
        Horizontal,
        Vertical,
        Radio
    }


    private T EnumToolbar<T>(T val) where T : struct, System.IComparable
    {
        return EnumToolbar(val, ToolbarStyle.Horizontal);
    }
    private T EnumToolbar<T>(T val, ToolbarStyle style) where T : struct, System.IComparable
    {
        var values = System.Enum.GetValues(typeof(T)) as T[];
        var names = System.Enum.GetNames(typeof(T));

        return EnumToolbar(val, names, values, style);
    }

    private T EnumToolbar<T>(T val, string[] names, T[] values) where T : struct, System.IComparable
    {
        return EnumToolbar(val, names, values, ToolbarStyle.Horizontal);
    }

    private T EnumToolbar<T>(T val, string[] names, T[] values, ToolbarStyle style) where T : struct, System.IComparable
    {
        int selected = -1;
        for (int i = 0; i < values.Length; ++i)
        {
            if (val.CompareTo(values[i]) == 0)
            {
                selected = i;
                break;
            }
        }

        switch (style)
        {
            case ToolbarStyle.Horizontal:
                selected = GUILayout.Toolbar(selected, names);
                if (selected >= 0)
                    val = values[selected];
                break;
            case ToolbarStyle.Vertical:
                selected = GUILayout.SelectionGrid(selected, names, 1);
                if (selected >= 0)
                    val = values[selected];
                break;
            case ToolbarStyle.Radio:
                GUILayout.BeginVertical();
                for (int i = 0; i < names.Length; ++i)
                {
                    bool pressed = GUILayout.Toggle(i == selected, names[i]);
                    if (pressed && i != selected)
                        val = values[i];
                }
                GUILayout.EndVertical();
                break;
        }
        return val;
    }

}
