import 'package:json_annotation/json_annotation.dart';

part 'clothing_item_assignment_history_model.g.dart';

@JsonSerializable()
class ClothingItemAssignmentHistoryModel {
  final String? id;
  final String itemId;
  final String personId;
  final DateTime assignedFrom;
  final DateTime? assignedUntil;

  ClothingItemAssignmentHistoryModel({
    this.id,
    required this.itemId,
    required this.personId,
    required this.assignedFrom,
    this.assignedUntil,
  });

  factory ClothingItemAssignmentHistoryModel.fromJson(Map<String, dynamic> json) => _$ClothingItemAssignmentHistoryModelFromJson(json);
  Map<String, dynamic> toJson() => _$ClothingItemAssignmentHistoryModelToJson(this);
}