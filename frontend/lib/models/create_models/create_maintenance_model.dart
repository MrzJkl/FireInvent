import 'package:flameguardlaundry/models/maintenance_type.dart';
import 'package:flameguardlaundry/models/user_model.dart';
import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_maintenance_model.g.dart';

@JsonSerializable()
@immutable
class CreateMaintenanceModel {
  final String itemId;
  final DateTime performedAt;
  final MaintenanceType maintenanceType;
  final String? remarks;
  final UserModel? performedBy;

  const CreateMaintenanceModel({
    required this.itemId,
    required this.performedAt,
    required this.maintenanceType,
    this.remarks,
    this.performedBy,
  });

  factory CreateMaintenanceModel.fromJson(Map<String, dynamic> json) =>
      _$CreateMaintenanceModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateMaintenanceModelToJson(this);
}
