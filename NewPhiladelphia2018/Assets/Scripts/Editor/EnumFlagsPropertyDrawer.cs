using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsPropertyDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		property.intValue = DrawEnumFlagsField(position, property.intValue, fieldInfo.FieldType, label);
	}

	public static int DrawEnumFlagsField(Rect position, int flags, System.Type enumType, GUIContent label) {
		var enumNames = System.Enum.GetNames(enumType);
		var enumValues = System.Enum.GetValues(enumType) as int[];

		List<string> itemNames = new List<string>();
		List<int> itemValues = new List<int>();

		int val = flags;
		int maskVal = 0;
		for (int i = 0; i < enumValues.Length; i++) {
			if (enumValues[i] != 0) {
				if ((val & enumValues[i]) == enumValues[i])
					maskVal |= 1 << itemValues.Count;
				itemNames.Add(enumNames[i]);
				itemValues.Add(enumValues[i]);
			}
		}
		int newMaskVal = EditorGUI.MaskField(position, label, maskVal, itemNames.ToArray());
		int changes = maskVal ^ newMaskVal;
		
		for (int i = 0; i < itemValues.Count; i++) {
			if ((changes & (1 << i)) != 0) { // has this list item changed?
				if ((newMaskVal & (1 << i)) != 0) { // has it been set?
					val |= itemValues[i];
				} else { // it has been reset
					val &= ~itemValues[i];
				}
			}
		}
		return val;
	}
}