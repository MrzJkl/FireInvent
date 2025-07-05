// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_department_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateDepartmentModel _$CreateDepartmentModelFromJson(
  Map<String, dynamic> json,
) => CreateDepartmentModel(
  name: json['name'] as String,
  description: json['description'] as String?,
);

Map<String, dynamic> _$CreateDepartmentModelToJson(
  CreateDepartmentModel instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
};
