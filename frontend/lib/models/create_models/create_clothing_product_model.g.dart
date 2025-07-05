// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_clothing_product_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateClothingProductModel _$CreateClothingProductModelFromJson(
  Map<String, dynamic> json,
) => CreateClothingProductModel(
  name: json['name'] as String,
  manufacturer: json['manufacturer'] as String,
  description: json['description'] as String?,
  type: $enumDecode(_$GearTypeEnumMap, json['type']),
);

Map<String, dynamic> _$CreateClothingProductModelToJson(
  CreateClothingProductModel instance,
) => <String, dynamic>{
  'name': instance.name,
  'manufacturer': instance.manufacturer,
  'description': instance.description,
  'type': _$GearTypeEnumMap[instance.type]!,
};

const _$GearTypeEnumMap = {
  GearType.jacket: 0,
  GearType.pants: 1,
  GearType.fireJacket: 2,
  GearType.firePants: 3,
  GearType.helmet: 4,
  GearType.boots: 5,
  GearType.gloves: 6,
  GearType.vest: 7,
  GearType.fireHood: 8,
  GearType.belt: 9,
};
