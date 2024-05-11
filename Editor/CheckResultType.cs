using System;
using UnityEditor;
using UnityEngine;

namespace GBG.AssetChecking.Editor
{
    public enum CheckResultType : uint
    {
        AllPass = 1U << 0,
        NotImportant = 1U << 1,
        Warning = 1U << 2,
        Error = 1U << 3,
        Exception = 1U << 4,
    }

    [Flags]
    public enum CheckResultTypes : uint
    {
#if UNITY_2022_3_OR_NEWER
        None = 0,
        AllTypes = ~0U, 
#endif

        // Sync with CheckResultType
        AllPass = CheckResultType.AllPass,
        NotImportant = CheckResultType.NotImportant,
        Warning = CheckResultType.Warning,
        Error = CheckResultType.Error,
        Exception = CheckResultType.Exception,
    }

    public static class CheckResultTypeHelper
    {
        public static Texture GetResultTypeIcon(this CheckResultType resultType, ResultIconStyle iconStyle)
        {
            switch (resultType)
            {
                case CheckResultType.AllPass:
                    switch (iconStyle)
                    {
                        case ResultIconStyle.Style1:
                            return EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo").image;
                        case ResultIconStyle.Style2:
                            return EditorGUIUtility.IconContent("d_winbtn_mac_max@2x").image;
                        case ResultIconStyle.Style3:
                            return EditorGUIUtility.IconContent("d_greenLight").image;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(iconStyle), iconStyle, null);
                    }
                case CheckResultType.NotImportant:
                    switch (iconStyle)
                    {
                        case ResultIconStyle.Style1:
                            return EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo").image;
                        case ResultIconStyle.Style2:
                            return EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo").image;
                        case ResultIconStyle.Style3:
                            return EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo").image;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(iconStyle), iconStyle, null);
                    }
                case CheckResultType.Warning:
                    switch (iconStyle)
                    {
                        case ResultIconStyle.Style1:
                            return EditorGUIUtility.IconContent("sv_icon_dot5_pix16_gizmo").image;
                        case ResultIconStyle.Style2:
                            return EditorGUIUtility.IconContent("d_winbtn_mac_min@2x").image;
                        case ResultIconStyle.Style3:
                            return EditorGUIUtility.IconContent("d_orangeLight").image;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(iconStyle), iconStyle, null);
                    }
                case CheckResultType.Error:
                    switch (iconStyle)
                    {
                        case ResultIconStyle.Style1:
                            return EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo").image;
                        case ResultIconStyle.Style2:
                            return EditorGUIUtility.IconContent("d_winbtn_mac_close@2x").image;
                        case ResultIconStyle.Style3:
                            return EditorGUIUtility.IconContent("d_redLight").image;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(iconStyle), iconStyle, null);
                    }
                case CheckResultType.Exception:
                    switch (iconStyle)
                    {
                        case ResultIconStyle.Style1:
                            return EditorGUIUtility.IconContent("sv_icon_dot7_pix16_gizmo").image;
                        case ResultIconStyle.Style2:
                            return EditorGUIUtility.IconContent("d_winbtn_mac_close_a@2x").image;
                        case ResultIconStyle.Style3:
                            return EditorGUIUtility.IconContent("Error@2x").image;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(iconStyle), iconStyle, null);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
            }
        }

        public static Color GetResultTypeBorderColor(this CheckResultType resultType)
        {
            switch (resultType)
            {
                case CheckResultType.AllPass:
                    return new Color(0 / 255f, 190 / 255f, 0 / 255f, 1.0f);
                case CheckResultType.NotImportant:
                    return new Color(229 / 255f, 229 / 255f, 229 / 255f, 1.0f);
                case CheckResultType.Warning:
                    return new Color(255 / 255f, 200 / 255f, 0 / 255f, 1.0f);
                case CheckResultType.Error:
                    return new Color(255 / 255f, 0 / 255f, 0 / 255f, 1.0f);
                case CheckResultType.Exception:
                    return new Color(150 / 255f, 0 / 255f, 0 / 255f, 1.0f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
            }
        }
    }
}