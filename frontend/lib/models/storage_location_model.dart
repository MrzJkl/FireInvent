import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_storage_location_model.dart';

part 'storage_location_model.g.dart';

@JsonSerializable()
@immutable
class StorageLocationModel extends CreateStorageLocationModel {
  final String id;

  const StorageLocationModel({
    required this.id,
    required super.name,
    super.remarks,
  });

  factory StorageLocationModel.fromJson(Map<String, dynamic> json) =>
      _$StorageLocationModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$StorageLocationModelToJson(this);
}
