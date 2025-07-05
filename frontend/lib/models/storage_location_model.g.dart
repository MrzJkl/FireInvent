// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'storage_location_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

StorageLocationModel _$StorageLocationModelFromJson(
  Map<String, dynamic> json,
) => StorageLocationModel(
  id: json['id'] as String,
  name: json['name'] as String,
  remarks: json['remarks'] as String?,
);

Map<String, dynamic> _$StorageLocationModelToJson(
  StorageLocationModel instance,
) => <String, dynamic>{
  'name': instance.name,
  'remarks': instance.remarks,
  'id': instance.id,
};
