import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_storage_location_model.g.dart';

@JsonSerializable()
@immutable
class CreateStorageLocationModel {
  final String name;
  final String? remarks;

  const CreateStorageLocationModel({required this.name, this.remarks});

  factory CreateStorageLocationModel.fromJson(Map<String, dynamic> json) =>
      _$CreateStorageLocationModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateStorageLocationModelToJson(this);
}
