import 'package:flameguardlaundry/models/create_clothing_item_assignment_history_model.dart';
import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'clothing_item_assignment_history_model.g.dart';

@JsonSerializable()
@immutable
class ClothingItemAssignmentHistoryModel
    extends CreateClothingItemAssignmentHistoryModel {
  final String id;

  const ClothingItemAssignmentHistoryModel({
    required this.id,
    required super.itemId,
    required super.personId,
    required super.assignedFrom,
    required super.assignedUntil,
  });

  factory ClothingItemAssignmentHistoryModel.fromJson(
    Map<String, dynamic> json,
  ) => _$ClothingItemAssignmentHistoryModelFromJson(json);
  Map<String, dynamic> toJson() =>
      _$ClothingItemAssignmentHistoryModelToJson(this);
}
