namespace Craiel.UnityGameData.Editor.Window
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builder;
    using Common;
    using Runtime.Contracts;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.Extensions;

    public class GameDataValidationWindow : EditorWindow
    {
        private static readonly GUILayoutOption ColumnWidth = GUILayout.Width(50);
        private static readonly GUILayoutOption SelectWidth = GUILayout.Width(100);

        private const string SelectText = "Select";

        private readonly IList<ValidationIssueGroup> issueGroups;

        private int fixableIssuesCount;

        private Texture2D errorIcon;
        private Texture2D warningIcon;

        private Vector2 scrollPosition;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataValidationWindow()
        {
            this.issueGroups = new List<ValidationIssueGroup>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static void OpenWindow()
        {
            var window = (GameDataValidationWindow)GetWindow(TypeCache<GameDataValidationWindow>.Value);
            window.titleContent = new GUIContent("Game Data Validation");
            window.Show();
        }

        public void Awake()
        {
            this.issueGroups.Clear();
            this.fixableIssuesCount = 0;
        }

        public void OnGUI()
        {
            this.errorIcon = EditorGUIUtility.FindTexture("d_console.erroricon.sml");
            this.warningIcon = EditorGUIUtility.FindTexture("d_console.warnicon.sml");

            this.DrawControls();
            this.DrawHeader();
            this.DrawResults();
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void Refresh()
        {
            this.fixableIssuesCount = 0;
            this.issueGroups.Clear();

            GameDataBuildValidationContext context = GameDataBuilder.Validate();

            // Process the data to better suit the display formatting
            var processingDictionary = new Dictionary<string, ValidationIssueGroup>();
            foreach (GameDataBuildValidationResult result in context.Errors)
            {
                this.ProcessResultEntry(processingDictionary, result, true);
            }

            foreach (GameDataBuildValidationResult result in context.Warnings)
            {
                this.ProcessResultEntry(processingDictionary, result, false);
            }

            CollectionExtensions.AddRange(this.issueGroups, processingDictionary.Values);

            foreach (ValidationIssueGroup issueGroup in this.issueGroups)
            {
                issueGroup.UpdateCanFix();
            }
        }

        private void ProcessResultEntry(IDictionary<string, ValidationIssueGroup> processingDictionary, GameDataBuildValidationResult result, bool isError)
        {
            ValidationIssueGroup entry;
            if (!processingDictionary.TryGetValue(result.RawMessage.ToLowerInvariant(), out entry))
            {
                entry = new ValidationIssueGroup(isError, result.RawMessage);
                processingDictionary.Add(result.RawMessage.ToLowerInvariant(), entry);
            }

            ValidationIssue issue = entry.Add(result);

            if (issue.CanApply)
            {
                this.fixableIssuesCount++;
            }
        }

        private void DrawControls()
        {
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
            {
                this.Refresh();
                return;
            }

            if (this.fixableIssuesCount > 0)
            {
                if (GUILayout.Button(string.Format("Fix {0} Issues", this.fixableIssuesCount), GUILayout.Width(150)))
                {
                    if (EditorUtility.DisplayDialog("Confirmation", "Fixing all issues could cause side effects, it is recommended to fix them individually.", "Do it anyway!", "Cancel"))
                    {
                        this.FixAllIssues();
                    }
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(2);
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Categories");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Count", ColumnWidth);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private void DrawResults()
        {
            if (this.issueGroups.Count == 0)
            {
                GUILayout.Label("No Results to display");
                return;
            }

            this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);

            foreach (ValidationIssueGroup result in this.issueGroups)
            {
                this.DrawContentResult(result);
            }

            GUILayout.EndScrollView();
        }

        private void DrawContentResult(ValidationIssueGroup issueGroup)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            GUILayout.Label(new GUIContent(issueGroup.IsError ? this.errorIcon : this.warningIcon));
            GUILayout.Label(issueGroup.RawMessage);
            GUILayout.FlexibleSpace();
            if (issueGroup.CanFix)
            {
                if (GUILayout.Button("Fix All", GUILayout.Width(70)))
                {
                    this.FixIssues(issueGroup);
                }
            }
            GUILayout.Label(issueGroup.Count.ToString(), ColumnWidth);
            GUILayout.EndHorizontal();

            var lastRect = GUILayoutUtility.GetLastRect();
            issueGroup.IsFoldout = GUI.Toggle(lastRect, issueGroup.IsFoldout, string.Empty);

            if (issueGroup.IsFoldout)
            {
                GUILayout.Space(10);
                for (var i = 0; i < issueGroup.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(20);

                    ValidationIssue issue = issueGroup.GetInfo(i);
                    GUILayout.Label(string.Format("[{0}]", issue.Source.GetType().Name), GUILayout.Width(200));
                    GUILayout.Label(issue.Message);

                    if (issue.CanApply)
                    {
                        if (GUILayout.Button("Fix", GUILayout.Width(50)))
                        {
                            this.FixIssue(issue);
                        }
                    }

                    if (issue.Owner == null)
                    {
                        GUILayout.Label("Unknown Owner", SelectWidth);
                    }
                    else
                    {
                        if (!this.DrawGameObjectLink(issue.Owner) && !this.DrawStaticDataRef(issue.Owner) && !this.DrawStaticDataLink(issue.Owner))
                        {
                            GUILayout.Label(issue.Owner.GetType().Name);
                        }
                    }


                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
            }
        }

        private bool DrawGameObjectLink(object source)
        {
            var gameObjectSource = source as GameObject;
            if (gameObjectSource != null)
            {
                if (GUILayout.Button(SelectText, SelectWidth))
                {
                    Selection.activeGameObject = gameObjectSource;
                }

                return true;
            }

            return false;
        }

        private bool DrawStaticDataRef(object source)
        {
            var staticDataRef = source as GameDataRefBase;
            if (staticDataRef != null)
            {
                if (GUILayout.Button(SelectText, SelectWidth))
                {
                    var window = GetWindow<GameDataEditorWindow>();
                    if (window != null)
                    {
                        window.SelectRef(staticDataRef);
                    }
                }


                return true;
            }

            return false;
        }

        private bool DrawStaticDataLink(object source)
        {
            var staticData = source as GameDataObject;
            if (staticData != null)
            {
                if (GUILayout.Button(SelectText, SelectWidth))
                {
                    var window = GetWindow<GameDataEditorWindow>();
                    if (window != null)
                    {
                        window.SelectRef(staticData);
                    }
                }

                return true;
            }

            return false;
        }

        private void FixAllIssues()
        {
            foreach (ValidationIssueGroup issueGroup in this.issueGroups)
            {
                this.FixIssues(issueGroup);
            }
        }

        private void FixIssues(ValidationIssueGroup issueGroup)
        {
            for (var i = 0; i < issueGroup.Count; i++)
            {
                this.FixIssue(issueGroup.GetInfo(i), true);
            }

            issueGroup.UpdateCanFix();
        }

        private void FixIssue(ValidationIssue issue, bool skipParentUpdate = false)
        {
            if (!issue.CanApply)
            {
                return;
            }

            this.fixableIssuesCount--;
            issue.ApplyFix(skipParentUpdate);
        }

        private class ValidationIssueGroup
        {
            private const int ResizeIncrement = 10;

            private ValidationIssue[] issues;

            public ValidationIssueGroup(bool isError, string rawMessage)
            {
                this.IsError = isError;

                this.RawMessage = rawMessage;

                this.issues = new ValidationIssue[ResizeIncrement];
            }

            public bool IsError { get; private set; }

            public string RawMessage { get; private set; }

            public int Count { get; private set; }

            public bool IsFoldout { get; set; }

            public bool CanFix { get; private set; }

            public ValidationIssue GetInfo(int index)
            {
                return this.issues[index];
            }

            public ValidationIssue Add(GameDataBuildValidationResult result)
            {
                if (this.Count >= this.issues.Length)
                {
                    Array.Resize(ref this.issues, this.Count + ResizeIncrement);
                }

                var issue = new ValidationIssue(this, result);
                this.issues[this.Count] = issue;

                this.Count++;
                return issue;
            }

            public void UpdateCanFix()
            {
                this.CanFix = this.issues.Any(x => x != null && x.CanApply);
            }
        }

        private class ValidationIssue
        {
            public ValidationIssue(ValidationIssueGroup parent, GameDataBuildValidationResult resultData)
            {
                this.Parent = parent;
                this.Message = resultData.FormattedMessage;
                this.Owner = resultData.Owner;
                this.Source = resultData.Source;
                this.Fix = resultData.FixDelegate;
            }

            public ValidationIssueGroup Parent { get; private set; }

            public string Message { get; private set; }

            public object Owner { get; private set; }

            public object Source { get; private set; }

            public bool CanApply
            {
                get { return !this.FixApplied && this.Fix != null; }
            }

            public GameDataValidationFixDelegate Fix { get; private set; }

            public bool FixApplied { get; private set; }

            public void ApplyFix(bool skipParentUpdate = false)
            {
                this.FixApplied = true;
                if (this.Fix.Invoke(this.Owner, this.Source))
                {
                    GameDataBuildValidationFixers.MarkOwnerDirty(this.Owner);
                }

                if (!skipParentUpdate)
                {
                    this.Parent.UpdateCanFix();
                }
            }
        }
    }
}