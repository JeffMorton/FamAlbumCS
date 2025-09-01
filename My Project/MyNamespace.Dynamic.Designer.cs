using System;
using System.ComponentModel;
using System.Diagnostics;

namespace FamAlbum.My
{
    internal static partial class MyProject
    {
        internal partial class MyForms
        {

            [EditorBrowsable(EditorBrowsableState.Never)]
            public AddPhoto m_AddPhoto;

            public AddPhoto AddPhoto
            {
                [DebuggerHidden]
                get
                {
                    m_AddPhoto = Create__Instance__(m_AddPhoto);
                    return m_AddPhoto;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_AddPhoto))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_AddPhoto);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public BackupRestore m_BackupRestore;

            public BackupRestore BackupRestore
            {
                [DebuggerHidden]
                get
                {
                    m_BackupRestore = Create__Instance__(m_BackupRestore);
                    return m_BackupRestore;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_BackupRestore))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_BackupRestore);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public Displayinfo m_Displayinfo;

            public Displayinfo Displayinfo
            {
                [DebuggerHidden]
                get
                {
                    m_Displayinfo = Create__Instance__(m_Displayinfo);
                    return m_Displayinfo;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_Displayinfo))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_Displayinfo);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public DisplayPics m_DisplayPics;

            public DisplayPics DisplayPics
            {
                [DebuggerHidden]
                get
                {
                    m_DisplayPics = Create__Instance__(m_DisplayPics);
                    return m_DisplayPics;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_DisplayPics))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_DisplayPics);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public EventManagmentType m_EventManagmentType;

            public EventManagmentType EventManagmentType
            {
                [DebuggerHidden]
                get
                {
                    m_EventManagmentType = Create__Instance__(m_EventManagmentType);
                    return m_EventManagmentType;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_EventManagmentType))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_EventManagmentType);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public GetDefaultFile m_GetDefaultFile;

            public GetDefaultFile GetDefaultFile
            {
                [DebuggerHidden]
                get
                {
                    m_GetDefaultFile = Create__Instance__(m_GetDefaultFile);
                    return m_GetDefaultFile;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_GetDefaultFile))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_GetDefaultFile);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public NameEditor m_NameEditor;

            public NameEditor NameEditor
            {
                [DebuggerHidden]
                get
                {
                    m_NameEditor = Create__Instance__(m_NameEditor);
                    return m_NameEditor;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_NameEditor))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_NameEditor);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public Select_Event m_Select_Event;

            public Select_Event Select_Event
            {
                [DebuggerHidden]
                get
                {
                    m_Select_Event = Create__Instance__(m_Select_Event);
                    return m_Select_Event;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_Select_Event))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_Select_Event);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public Start m_Start;

            public Start Start
            {
                [DebuggerHidden]
                get
                {
                    m_Start = Create__Instance__(m_Start);
                    return m_Start;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_Start))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_Start);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public Sthumb m_Sthumb;

            public Sthumb Sthumb
            {
                [DebuggerHidden]
                get
                {
                    m_Sthumb = Create__Instance__(m_Sthumb);
                    return m_Sthumb;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_Sthumb))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_Sthumb);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public Thum m_Thum;

            public Thum Thum
            {
                [DebuggerHidden]
                get
                {
                    m_Thum = Create__Instance__(m_Thum);
                    return m_Thum;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_Thum))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_Thum);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public ThumnaliExtractor m_ThumnaliExtractor;

            public ThumnaliExtractor ThumnaliExtractor
            {
                [DebuggerHidden]
                get
                {
                    m_ThumnaliExtractor = Create__Instance__(m_ThumnaliExtractor);
                    return m_ThumnaliExtractor;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_ThumnaliExtractor))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_ThumnaliExtractor);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public VPlayer m_VPlayer;

            public VPlayer VPlayer
            {
                [DebuggerHidden]
                get
                {
                    m_VPlayer = Create__Instance__(m_VPlayer);
                    return m_VPlayer;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_VPlayer))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_VPlayer);
                }
            }


            [EditorBrowsable(EditorBrowsableState.Never)]
            public working m_working;

            public working working
            {
                [DebuggerHidden]
                get
                {
                    m_working = Create__Instance__(m_working);
                    return m_working;
                }
                [DebuggerHidden]
                set
                {
                    if (ReferenceEquals(value, m_working))
                        return;
                    if (value is not null)
                        throw new ArgumentException("Property can only be set to Nothing");
                    Dispose__Instance__(ref m_working);
                }
            }

        }


    }
}