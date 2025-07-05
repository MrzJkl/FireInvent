import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_department_model.dart';

part 'department_model.g.dart';

@JsonSerializable()
@immutable
class DepartmentModel extends CreateDepartmentModel {
  final String id;

  const DepartmentModel({
    required this.id,
    required super.name,
    super.description,
  });

  factory DepartmentModel.fromJson(Map<String, dynamic> json) =>
      _$DepartmentModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$DepartmentModelToJson(this);
}
