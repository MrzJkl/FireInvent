import 'package:json_annotation/json_annotation.dart';

import 'user_model.dart';

part 'maintenance_model.g.dart';

enum MaintenanceType {
  @JsonValue(0)
  standard,
}

@JsonSerializable()
class MaintenanceModel {
  final String? id;
  final String itemId;
  final DateTime performedAt;
  final MaintenanceType maintenanceType;
  final String? remarks;
  final UserModel? performedBy;

  MaintenanceModel({
    this.id,
    required this.itemId,
    required this.performedAt,
    required this.maintenanceType,
    this.remarks,
    this.performedBy,
  });

  factory MaintenanceModel.fromJson(Map<String, dynamic> json) => _$MaintenanceModelFromJson(json);
  Map<String, dynamic> toJson() => _$MaintenanceModelToJson(this);
}
