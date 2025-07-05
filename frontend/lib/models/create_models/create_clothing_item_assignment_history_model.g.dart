// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_clothing_item_assignment_history_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateClothingItemAssignmentHistoryModel
_$CreateClothingItemAssignmentHistoryModelFromJson(Map<String, dynamic> json) =>
    CreateClothingItemAssignmentHistoryModel(
      itemId: json['itemId'] as String,
      personId: json['personId'] as String,
      assignedFrom: DateTime.parse(json['assignedFrom'] as String),
      assignedUntil:
          json['assignedUntil'] == null
              ? null
              : DateTime.parse(json['assignedUntil'] as String),
    );

Map<String, dynamic> _$CreateClothingItemAssignmentHistoryModelToJson(
  CreateClothingItemAssignmentHistoryModel instance,
) => <String, dynamic>{
  'itemId': instance.itemId,
  'personId': instance.personId,
  'assignedFrom': instance.assignedFrom.toIso8601String(),
  'assignedUntil': instance.assignedUntil?.toIso8601String(),
};
