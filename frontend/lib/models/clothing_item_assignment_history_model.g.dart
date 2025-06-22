// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'clothing_item_assignment_history_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClothingItemAssignmentHistoryModel _$ClothingItemAssignmentHistoryModelFromJson(
  Map<String, dynamic> json,
) => ClothingItemAssignmentHistoryModel(
  id: json['id'] as String?,
  itemId: json['itemId'] as String,
  personId: json['personId'] as String,
  assignedFrom: DateTime.parse(json['assignedFrom'] as String),
  assignedUntil:
      json['assignedUntil'] == null
          ? null
          : DateTime.parse(json['assignedUntil'] as String),
);

Map<String, dynamic> _$ClothingItemAssignmentHistoryModelToJson(
  ClothingItemAssignmentHistoryModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'itemId': instance.itemId,
  'personId': instance.personId,
  'assignedFrom': instance.assignedFrom.toIso8601String(),
  'assignedUntil': instance.assignedUntil?.toIso8601String(),
};
