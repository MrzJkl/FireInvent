// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_maintenance_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateMaintenanceModel _$CreateMaintenanceModelFromJson(
  Map<String, dynamic> json,
) => CreateMaintenanceModel(
  itemId: json['itemId'] as String,
  performedAt: DateTime.parse(json['performedAt'] as String),
  maintenanceType: $enumDecode(
    _$MaintenanceTypeEnumMap,
    json['maintenanceType'],
  ),
  remarks: json['remarks'] as String?,
  performedBy:
      json['performedBy'] == null
          ? null
          : UserModel.fromJson(json['performedBy'] as Map<String, dynamic>),
);

Map<String, dynamic> _$CreateMaintenanceModelToJson(
  CreateMaintenanceModel instance,
) => <String, dynamic>{
  'itemId': instance.itemId,
  'performedAt': instance.performedAt.toIso8601String(),
  'maintenanceType': _$MaintenanceTypeEnumMap[instance.maintenanceType]!,
  'remarks': instance.remarks,
  'performedBy': instance.performedBy,
};

const _$MaintenanceTypeEnumMap = {MaintenanceType.washing: 0};
