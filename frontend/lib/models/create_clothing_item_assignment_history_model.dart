import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_clothing_item_assignment_history_model.g.dart';

@JsonSerializable()
@immutable
class CreateClothingItemAssignmentHistoryModel {
  final String itemId;
  final String personId;
  final DateTime assignedFrom;
  final DateTime? assignedUntil;

  const CreateClothingItemAssignmentHistoryModel({
    required this.itemId,
    required this.personId,
    required this.assignedFrom,
    required this.assignedUntil,
  });
}
