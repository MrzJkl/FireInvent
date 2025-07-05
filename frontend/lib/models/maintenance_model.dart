import 'package:flameguardlaundry/models/maintenance_type.dart';
import 'package:flameguardlaundry/models/user_model.dart';
import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_maintenance_model.dart';

part 'maintenance_model.g.dart';

@JsonSerializable()
@immutable
class MaintenanceModel extends CreateMaintenanceModel {
  final String id;

  const MaintenanceModel({
    required this.id,
    required super.itemId,
    required super.performedAt,
    required super.maintenanceType,
    super.remarks,
    super.performedBy,
  });

  factory MaintenanceModel.fromJson(Map<String, dynamic> json) =>
      _$MaintenanceModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$MaintenanceModelToJson(this);
}
