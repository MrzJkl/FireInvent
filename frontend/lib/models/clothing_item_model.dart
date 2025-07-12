import 'package:fireinvent/models/gear_condition.dart';
import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_clothing_item_model.dart';

part 'clothing_item_model.g.dart';

@JsonSerializable()
@immutable
class ClothingItemModel extends CreateClothingItemModel {
  final String id;

  const ClothingItemModel({
    required this.id,
    required super.variantId,
    super.identifier,
    super.storageLocationId,
    required super.condition,
    required super.purchaseDate,
    required super.retirementDate,
  });

  factory ClothingItemModel.fromJson(Map<String, dynamic> json) =>
      _$ClothingItemModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$ClothingItemModelToJson(this);
}
