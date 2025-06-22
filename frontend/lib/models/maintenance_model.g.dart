// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'maintenance_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

MaintenanceModel _$MaintenanceModelFromJson(Map<String, dynamic> json) =>
    MaintenanceModel(
      id: json['id'] as String?,
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

Map<String, dynamic> _$MaintenanceModelToJson(MaintenanceModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'itemId': instance.itemId,
      'performedAt': instance.performedAt.toIso8601String(),
      'maintenanceType': _$MaintenanceTypeEnumMap[instance.maintenanceType]!,
      'remarks': instance.remarks,
      'performedBy': instance.performedBy,
    };

const _$MaintenanceTypeEnumMap = {MaintenanceType.standard: 0};
