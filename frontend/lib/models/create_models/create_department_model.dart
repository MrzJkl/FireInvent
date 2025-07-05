import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_department_model.g.dart';

@JsonSerializable()
@immutable
class CreateDepartmentModel {
  final String name;
  final String? description;

  const CreateDepartmentModel({required this.name, this.description});

  factory CreateDepartmentModel.fromJson(Map<String, dynamic> json) =>
      _$CreateDepartmentModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateDepartmentModelToJson(this);
}
